using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Models;
using API.Tests.Helpers;

namespace API.Tests.Integration;

public class AdminIntegrationTests : IClassFixture<GatherlyApiFactory>
{
    private readonly GatherlyApiFactory _factory;

    public AdminIntegrationTests(GatherlyApiFactory factory)
    {
        _factory = factory;
    }

    private User SeedUser(bool isAdmin = false)
    {
        var user = TestData.User(email: $"{Guid.NewGuid():N}@email.com");
        user.IsAdmin = isAdmin;
        _factory.Seed(db => db.Users.Add(user));
        return user;
    }

    private async Task<(HttpClient client, string token)> Login(User user)
    {
        var anonymous = _factory.CreateClient();
        var login = await anonymous.PostAsJsonAsync("/api/login", new { email = user.Email, password = "123456" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    private static JsonElement DecodePayload(string token)
    {
        var part = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        part = part.PadRight(part.Length + (4 - part.Length % 4) % 4, '=');
        return JsonDocument.Parse(Convert.FromBase64String(part)).RootElement;
    }

    [Fact]
    public async Task Admin_SemToken_Retorna401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UsuarioComum_Retorna403()
    {
        var (client, token) = await Login(SeedUser());

        var response = await client.GetAsync("/api/admin/stats");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False(DecodePayload(token).TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out _));
    }

    [Fact]
    public async Task Admin_TokenTemRoleEAcessaEndpoints()
    {
        var (client, token) = await Login(SeedUser(isAdmin: true));

        Assert.Equal("Admin", DecodePayload(token).GetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").GetString());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/reports?status=Pending")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/users?search=a")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/events")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/comments")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/admin/reports?status=Invalido")).StatusCode);
    }

    [Fact]
    public async Task SuspenderUsuario_TokenDeleParaDeFuncionarNaHora()
    {
        var (adminClient, _) = await Login(SeedUser(isAdmin: true));
        var target = SeedUser();
        var (targetClient, _) = await Login(target);

        Assert.Equal(HttpStatusCode.OK, (await targetClient.GetAsync("/api/user/me")).StatusCode);

        var suspend = await adminClient.PatchAsJsonAsync($"/api/admin/users/{target.Id}/suspended", new { value = true });
        Assert.Equal(HttpStatusCode.NoContent, suspend.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await targetClient.GetAsync("/api/user/me")).StatusCode);
    }

    [Fact]
    public async Task RemoverAdmin_PerdeAcessoNaHora()
    {
        var (adminClient, _) = await Login(SeedUser(isAdmin: true));
        var other = SeedUser(isAdmin: true);
        var (otherClient, _) = await Login(other);

        Assert.Equal(HttpStatusCode.OK, (await otherClient.GetAsync("/api/admin/stats")).StatusCode);

        var demote = await adminClient.PatchAsJsonAsync($"/api/admin/users/{other.Id}/admin", new { value = false });
        Assert.Equal(HttpStatusCode.NoContent, demote.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await otherClient.GetAsync("/api/admin/stats")).StatusCode);
    }

    [Fact]
    public async Task AcaoInvalida_Retorna400ComErros()
    {
        var admin = SeedUser(isAdmin: true);
        var (client, _) = await Login(admin);

        var response = await client.PatchAsJsonAsync($"/api/admin/users/{admin.Id}/suspended", new { value = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Você não pode suspender a si mesmo.", body.GetProperty("errors")[0].GetString());
    }

    [Fact]
    public async Task UsuarioApagadoDoBanco_TokenRetorna401()
    {
        var user = SeedUser();
        var (client, _) = await Login(user);

        _factory.Seed(db => db.Users.Remove(db.Users.Single(u => u.Id == user.Id)));

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/user/me")).StatusCode);
    }
}
