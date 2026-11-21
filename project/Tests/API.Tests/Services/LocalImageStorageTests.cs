using API.Services;
using API.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Services;

public class LocalImageStorageTests : IDisposable
{
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "gatherly-tests-" + Guid.NewGuid());
    private readonly Mock<IHttpContextAccessor> _accessor = new();

    private LocalImageStorage CreateStorage(bool withRequest = true)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.WebRootPath).Returns(_webRoot);

        if (withRequest)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("api.gatherly.com");
            _accessor.SetupGet(a => a.HttpContext).Returns(context);
        }

        return new LocalImageStorage(env.Object, _accessor.Object, NullLogger<LocalImageStorage>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRoot)) Directory.Delete(_webRoot, recursive: true);
    }

    [Fact]
    public async Task SaveAsync_GravaArquivoERetornaUrlPublica()
    {
        var url = await CreateStorage().SaveAsync(FormFileFactory.ValidJpeg("Foto.JPG"), "events");

        Assert.StartsWith("https://api.gatherly.com/uploads/events/", url);
        Assert.EndsWith(".jpg", url);
        var fileName = url.Split('/').Last();
        Assert.True(File.Exists(Path.Combine(_webRoot, "uploads", "events", fileName)));
    }

    [Fact]
    public async Task SaveAsync_IgnoraTentativaDePathTraversalNaPasta()
    {
        var url = await CreateStorage().SaveAsync(FormFileFactory.ValidJpeg(), "../../etc");

        Assert.Contains("/uploads/etc/", url);
        Assert.True(Directory.Exists(Path.Combine(_webRoot, "uploads", "etc")));
    }

    [Fact]
    public async Task SaveAsync_SemRequisicaoHttp_Lanca()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateStorage(withRequest: false).SaveAsync(FormFileFactory.ValidJpeg(), "events"));
    }

    [Fact]
    public async Task DeleteAsync_RemoveArquivoExistente()
    {
        var storage = CreateStorage();
        var url = await storage.SaveAsync(FormFileFactory.ValidJpeg(), "users");
        var path = Path.Combine(_webRoot, "uploads", "users", url.Split('/').Last());

        await storage.DeleteAsync(url);

        Assert.False(File.Exists(path));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://outro-site.com/imagem.jpg")]
    [InlineData("https://api.gatherly.com/uploads/somente-um-segmento")]
    [InlineData("https://api.gatherly.com/uploads/users/inexistente.jpg")]
    public async Task DeleteAsync_UrlsInvalidasOuInexistentes_NaoLanca(string? url)
    {
        var exception = await Record.ExceptionAsync(() => CreateStorage().DeleteAsync(url));
        Assert.Null(exception);
    }
}
