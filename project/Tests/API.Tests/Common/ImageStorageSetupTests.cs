using API.Common;
using API.Interfaces;
using API.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Common;

public class ImageStorageSetupTests
{
    private static IConfiguration Config(string? connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureStorage:ConnectionString"] = connectionString
            })
            .Build();

    private static Type? RegisteredStorage(IServiceCollection services) =>
        Assert.Single(services, s => s.ServiceType == typeof(IImageStorage)).ImplementationType;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SemConnectionString_UsaArmazenamentoEmDisco(string? connectionString)
    {
        var services = new ServiceCollection();

        services.AddImageStorage(Config(connectionString));

        Assert.Equal(typeof(LocalImageStorage), RegisteredStorage(services));
    }

    [Fact]
    public void ComConnectionString_UsaBlobStorageNoContainerUploads()
    {
        var services = new ServiceCollection();

        services.AddImageStorage(Config("UseDevelopmentStorage=true"));

        Assert.Equal(typeof(BlobImageStorage), RegisteredStorage(services));

        var container = services.BuildServiceProvider().GetRequiredService<BlobContainerClient>();
        Assert.Equal("uploads", container.Name);
    }
}
