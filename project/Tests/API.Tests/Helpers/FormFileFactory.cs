using Microsoft.AspNetCore.Http;

namespace API.Tests.Helpers;

public static class FormFileFactory
{
    public static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
    public static readonly byte[] WebpHeader = "RIFF\0\0\0\0WEBP"u8.ToArray();

    public static IFormFile Create(byte[] content, string fileName, string contentType, long? length = null)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, length ?? content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public static IFormFile ValidJpeg(string fileName = "foto.jpg") => Create(JpegHeader, fileName, "image/jpeg");
}
