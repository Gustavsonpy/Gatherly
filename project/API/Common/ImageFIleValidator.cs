namespace API.Common
{
    public static class ImageFileValidator
    {
        public const long MaxSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static string? Validate(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return "Nenhum arquivo enviado";

            if (file.Length > MaxSizeBytes)
                return "A imagem deve ter no máximo 5 MB";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
                return "Formato de imagem inválido";

            return null;
        }
    }
}