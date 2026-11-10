
using SixLabors.ImageSharp;

namespace API.Common
{
    public static class ImageFileValidator
    {
        public const long MaxSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static async Task<string?> ValidateAsync(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return "Nenhum arquivo enviado";

            if (file.Length > MaxSizeBytes)
                return "A imagem deve ter no máximo 5 MB";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) ||
                file.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            {
                return "Formato de imagem inválido";
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var imageInfo = await Image.IdentifyAsync(stream);

                if (imageInfo is null)
                    return "O arquivo enviado não é uma imagem válida";

                var format = imageInfo.Metadata.DecodedImageFormat;

                var validFormat = format?.Name.ToLowerInvariant() switch
                {
                    "jpeg" => extension is ".jpg" or ".jpeg"
                        && file.ContentType == "image/jpeg",

                    "png" => extension == ".png"
                        && file.ContentType == "image/png",

                    "webp" => extension == ".webp"
                        && file.ContentType == "image/webp",

                    _ => false
                };

                if (!validFormat)
                    return "O conteúdo do arquivo não corresponde ao formato informado";
            }
            catch (UnknownImageFormatException)
            {
                return "O arquivo enviado não é uma imagem válida";
            }
            catch (InvalidImageContentException)
            {
                return "O arquivo enviado não é uma imagem válida";
            }

            return null;
        }
    }
}
