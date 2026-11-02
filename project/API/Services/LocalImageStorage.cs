using API.Interfaces;

namespace API.Services
{
    public class LocalImageStorage : IImageStorage
    {
        private const string UploadsSegment = "uploads";

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LocalImageStorage> _logger;

        public LocalImageStorage(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LocalImageStorage> logger)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException("Não há requisição HTTP ativa para montar a URL da imagem.");

            var safeFolder = Path.GetFileName(folder);
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var directory = Path.Combine(_env.WebRootPath, UploadsSegment, safeFolder);
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(directory, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            return $"{request.Scheme}://{request.Host}/{UploadsSegment}/{safeFolder}/{fileName}";
        }

        public Task DeleteAsync(string? url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Task.CompletedTask;

            var marker = $"/{UploadsSegment}/";
            var index = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return Task.CompletedTask;

            var parts = url[(index + marker.Length)..].Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                return Task.CompletedTask;

            var fullPath = Path.Combine(
                _env.WebRootPath,
                UploadsSegment,
                Path.GetFileName(parts[0]),
                Path.GetFileName(parts[1]));

            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Não foi possível apagar a imagem {Path}", fullPath);
            }

            return Task.CompletedTask;
        }
    }
}