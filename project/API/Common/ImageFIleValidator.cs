
namespace API.Common
{
    public static class ImageFileValidator
    {
        public const long MaxSizeBytes = 5 * 1024 * 1024;

        private static readonly Dictionary<string, (byte[] Signature, string ContentType)> AllowedFormats = new()
        {
            [".jpg"] = (new byte[] { 0xFF, 0xD8, 0xFF }, "image/jpeg"),
            [".jpeg"] = (new byte[] { 0xFF, 0xD8, 0xFF }, "image/jpeg"),
            [".png"] = (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "image/png"),
            [".webp"] = (new byte[] { 0x52, 0x49, 0x46, 0x46 }, "image/webp")
        };

        public static string? Validate(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return "Nenhum arquivo enviado";

            if (file.Length > MaxSizeBytes)
                return "A imagem deve ter no máximo 5 MB";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedFormats.TryGetValue(extension, out var format))
                return "Formato de imagem inválido";

            if (file.ContentType != format.ContentType)
                return "Formato de imagem inválido";

            try
            {
                using var stream = file.OpenReadStream();

                var header = new byte[12];
                stream.ReadExactly(header);

                var signatureMatches = header
                    .Take(format.Signature.Length)
                    .SequenceEqual(format.Signature);

                if (!signatureMatches)
                    return "O conteúdo do arquivo não corresponde ao formato informado";

                if (extension == ".webp" &&
                    !header.Skip(8).Take(4).SequenceEqual("WEBP"u8.ToArray()))
                {
                    return "O conteúdo do arquivo não corresponde ao formato informado";
                }
            }
            catch (EndOfStreamException)
            {
                return "O arquivo enviado não é uma imagem válida";
            }

            return null;
        }
    }
}
