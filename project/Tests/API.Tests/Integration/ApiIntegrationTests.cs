using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Models;
using API.Tests.Helpers;

namespace API.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<GatherlyApiFactory>
{
    private readonly GatherlyApiFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(GatherlyApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<HttpClient> AuthenticatedClient(string email)
    {
        var login = await _client.PostAsJsonAsync("/api/login", new { email, password = "123456" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Health_RetornaOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthDb_ConectaAoBanco()
    {
        var response = await _client.GetAsync("/health/db");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_Retorna401()
    {
        var response = await _client.GetAsync("/api/event");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ModelStateInvalido_RetornaErrosPorCampoEmCamelCase()
    {
        var response = await _client.PostAsJsonAsync("/api/user/create", new
        {
            name = "Ana",
            email = "email-invalido",
            password = "123",
            birthday = "2000-01-01"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var fieldErrors = body.GetProperty("fieldErrors");
        Assert.True(fieldErrors.TryGetProperty("email", out _));
        Assert.True(fieldErrors.TryGetProperty("password", out _));
        Assert.True(body.GetProperty("errors").GetArrayLength() >= 2);
    }

    [Fact]
    public async Task FluxoCompleto_CadastroLoginCriarEventoEComentar()
    {
        var category = TestData.Category("Integração-" + Guid.NewGuid().ToString("N")[..6]);
        _factory.Seed(db => db.Categories.Add(category));
        var email = $"fluxo-{Guid.NewGuid():N}@email.com";

        var register = await _client.PostAsJsonAsync("/api/user/create", new
        {
            name = "Ana Integração", email, password = "123456", birthday = "2000-01-01"
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var client = await AuthenticatedClient(email);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/user/me");
        Assert.Equal("Ana Integração", me.GetProperty("name").GetString());

        var create = await client.PostAsJsonAsync("/api/event/create", new
        {
            title = "Evento de integração",
            description = "Teste",
            dateTimeStart = DateTime.UtcNow.AddDays(3),
            dateTimeEnd = DateTime.UtcNow.AddDays(3).AddHours(2),
            localization = "Praça",
            maxCapacity = 5,
            city = "Curitiba",
            urlImage = "http://localhost/uploads/events/a.jpg",
            categoryId = category.Id
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var eventId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/event/{eventId}");
        Assert.Equal(1, detail.GetProperty("participantsCount").GetInt32());
        Assert.True(detail.GetProperty("isParticipating").GetBoolean());

        var comment = await client.PostAsJsonAsync("/api/comment/create", new { text = "Bora!", eventId });
        Assert.Equal(HttpStatusCode.OK, comment.StatusCode);

        var comments = await _client.GetFromJsonAsync<JsonElement>($"/api/comment/{eventId}");
        Assert.Equal("Bora!", comments[0].GetProperty("text").GetString());

        var leave = await client.DeleteAsync($"/api/event/{eventId}/join");
        Assert.Equal(HttpStatusCode.BadRequest, leave.StatusCode);

        var cancel = await client.PostAsync($"/api/event/{eventId}/cancel", null);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
    }

    [Fact]
    public async Task EndpointsPublicos_RespondemSemAutenticacao()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/category")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/report-reasons")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/rate/{Guid.NewGuid()}/score")).StatusCode);
    }
}
