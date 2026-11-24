using API.Services;
using API.Tests.Helpers;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Services;

public class BlobImageStorageTests
{
    private static readonly Uri ContainerUri = new("https://conta.blob.core.windows.net/uploads");

    private readonly Mock<BlobContainerClient> _container = new();
    private readonly Mock<BlobClient> _blob = new();

    public BlobImageStorageTests()
    {
        _container.SetupGet(c => c.Uri).Returns(ContainerUri);
    }

    private BlobImageStorage CreateStorage() =>
        new(_container.Object, NullLogger<BlobImageStorage>.Instance);

    [Fact]
    public async Task SaveAsync_EnviaParaAPastaComNomeUnicoERetornaUrlDoBlob()
    {
        string? blobName = null;
        _container.Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => blobName = name)
            .Returns(_blob.Object);
        _blob.SetupGet(b => b.Uri).Returns(new Uri("https://conta.blob.core.windows.net/uploads/events/x.jpg"));

        var url = await CreateStorage().SaveAsync(FormFileFactory.ValidJpeg("Foto.JPG"), "events");

        Assert.Matches(@"^events/[0-9a-f-]{36}\.jpg$", blobName);
        Assert.Equal("https://conta.blob.core.windows.net/uploads/events/x.jpg", url);
    }

    [Fact]
    public async Task SaveAsync_EnviaComOContentTypeDaImagem()
    {
        _container.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(_blob.Object);
        _blob.SetupGet(b => b.Uri).Returns(new Uri("https://conta.blob.core.windows.net/uploads/events/x.jpg"));

        await CreateStorage().SaveAsync(FormFileFactory.ValidJpeg(), "events");

        _blob.Verify(b => b.UploadAsync(
            It.IsAny<Stream>(),
            It.Is<BlobUploadOptions>(o => o.HttpHeaders.ContentType == "image/jpeg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_PastaComCaminho_UsaSoONomeDaPasta()
    {
        string? blobName = null;
        _container.Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => blobName = name)
            .Returns(_blob.Object);
        _blob.SetupGet(b => b.Uri).Returns(new Uri("https://conta.blob.core.windows.net/uploads/users/x.jpg"));

        await CreateStorage().SaveAsync(FormFileFactory.ValidJpeg(), "../../users");

        Assert.StartsWith("users/", blobName);
    }

    [Fact]
    public async Task DeleteAsync_UrlDoContainer_ApagaOBlob()
    {
        await CreateStorage().DeleteAsync("https://conta.blob.core.windows.net/uploads/events/abc.jpg");

        _container.Verify(c => c.DeleteBlobIfExistsAsync(
            "events/abc.jpg",
            It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://localhost:5008/uploads/events/abc.jpg")]
    public async Task DeleteAsync_UrlVaziaOuDeOutraOrigem_NaoApaga(string? url)
    {
        await CreateStorage().DeleteAsync(url);

        _container.Verify(c => c.DeleteBlobIfExistsAsync(
            It.IsAny<string>(),
            It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ErroNaAzure_NaoLancaExcecao()
    {
        _container.Setup(c => c.DeleteBlobIfExistsAsync(
                It.IsAny<string>(),
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("falhou"));

        var exception = await Record.ExceptionAsync(() =>
            CreateStorage().DeleteAsync("https://conta.blob.core.windows.net/uploads/events/abc.jpg"));

        Assert.Null(exception);
    }
}
