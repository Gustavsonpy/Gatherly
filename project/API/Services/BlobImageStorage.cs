using API.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace API.Services
{
    /// <summary>
    /// Salva as imagens no Azure Blob Storage. Com o container "uploads", a URL fica
    /// https://conta.blob.core.windows.net/uploads/events/guid.jpg, com o mesmo caminho
    /// "/uploads/pasta/" que o LocalImageStorage gera.
    /// </summary>
    public class BlobImageStorage : IImageStorage
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<BlobImageStorage> _logger;

        public BlobImageStorage(BlobContainerClient container, ILogger<BlobImageStorage> logger)
        {
            _container = container;
            _logger = logger;
        }

        public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            // Mesmo padrão do LocalImageStorage: pasta/guid.extensão
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{Path.GetFileName(folder)}/{Guid.NewGuid()}{extension}";
            var blob = _container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                // Sem o ContentType o navegador baixa o arquivo em vez de exibir a imagem
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            }, ct);

            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string? url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            // Só apaga imagens do container (ignora, por exemplo, as antigas salvas em disco)
            var prefix = _container.Uri.AbsoluteUri.TrimEnd('/') + "/";

            if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return;

            var blobName = Uri.UnescapeDataString(url[prefix.Length..]);

            try
            {
                await _container.DeleteBlobIfExistsAsync(blobName, cancellationToken: ct);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogWarning(ex, "Não foi possível apagar a imagem {BlobName}", blobName);
            }
        }
    }
}
