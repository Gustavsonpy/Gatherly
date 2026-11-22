using API.Interfaces;
using API.Services;
using Azure.Storage.Blobs;

namespace API.Common
{
    public static class ImageStorageSetup
    {
        public const string BlobContainerName = "uploads";

        /// <summary>
        /// Com "AzureStorage:ConnectionString" configurada, as imagens vão para o Azure Blob Storage;
        /// sem ela (ex.: rodando local), continuam sendo salvas em disco (wwwroot/uploads).
        /// </summary>
        public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddScoped<IImageStorage, LocalImageStorage>();
            }
            else
            {
                services.AddSingleton(new BlobContainerClient(connectionString, BlobContainerName));
                services.AddScoped<IImageStorage, BlobImageStorage>();
            }

            return services;
        }
    }
}
