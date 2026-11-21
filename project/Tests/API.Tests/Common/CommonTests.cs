using API.Common;
using API.Tests.Helpers;

namespace API.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_GuardaValorSemErros()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
        Assert.Empty(result.FieldErrors);
    }

    [Fact]
    public void Failure_ComMensagemOuLista()
    {
        Assert.Equal(new[] { "x" }, Result<int>.Failure("x").Errors);
        Assert.Equal(new[] { "a", "b" }, Result<int>.Failure(new List<string> { "a", "b" }).Errors);
        Assert.False(Result<int>.Failure("x").IsSuccess);
    }

    [Fact]
    public void Failure_ComErrosDeCampo_AchataMensagens()
    {
        var fields = new Dictionary<string, List<string>>
        {
            ["name"] = new() { "n1" },
            ["email"] = new() { "e1", "e2" }
        };

        var result = Result<string>.Failure(fields);

        Assert.Equal(new[] { "n1", "e1", "e2" }, result.Errors);
        Assert.Same(fields, result.FieldErrors);
    }
}

public class ImageFileValidatorTests
{
    [Fact]
    public void ArquivoNuloOuVazio_RetornaErro()
    {
        Assert.Equal("Nenhum arquivo enviado", ImageFileValidator.Validate(null));
        Assert.Equal("Nenhum arquivo enviado", ImageFileValidator.Validate(FormFileFactory.Create(Array.Empty<byte>(), "a.jpg", "image/jpeg")));
    }

    [Fact]
    public void ArquivoMaiorQue5MB_RetornaErro()
    {
        var file = FormFileFactory.Create(FormFileFactory.JpegHeader, "a.jpg", "image/jpeg", ImageFileValidator.MaxSizeBytes + 1);
        Assert.Equal("A imagem deve ter no máximo 5 MB", ImageFileValidator.Validate(file));
    }

    [Theory]
    [InlineData("a.gif", "image/gif")]
    [InlineData("a.jpg", "image/png")]
    public void ExtensaoOuContentTypeInvalido_RetornaErro(string name, string contentType)
    {
        var file = FormFileFactory.Create(FormFileFactory.JpegHeader, name, contentType);
        Assert.Equal("Formato de imagem inválido", ImageFileValidator.Validate(file));
    }

    [Fact]
    public void AssinaturaNaoCorresponde_RetornaErro()
    {
        var file = FormFileFactory.Create(FormFileFactory.PngHeader, "a.jpg", "image/jpeg");
        Assert.Equal("O conteúdo do arquivo não corresponde ao formato informado", ImageFileValidator.Validate(file));
    }

    [Fact]
    public void WebpSemMarcadorWEBP_RetornaErro()
    {
        var content = "RIFF\0\0\0\0AVI "u8.ToArray();
        var file = FormFileFactory.Create(content, "a.webp", "image/webp");
        Assert.Equal("O conteúdo do arquivo não corresponde ao formato informado", ImageFileValidator.Validate(file));
    }

    [Fact]
    public void ArquivoMenorQueOCabecalho_RetornaErro()
    {
        var file = FormFileFactory.Create(new byte[] { 0xFF, 0xD8 }, "a.jpg", "image/jpeg");
        Assert.Equal("O arquivo enviado não é uma imagem válida", ImageFileValidator.Validate(file));
    }

    [Theory]
    [InlineData("a.jpg", "image/jpeg")]
    [InlineData("a.JPEG", "image/jpeg")]
    [InlineData("a.png", "image/png")]
    [InlineData("a.webp", "image/webp")]
    public void ImagensValidas_RetornaNull(string name, string contentType)
    {
        var header = contentType switch
        {
            "image/png" => FormFileFactory.PngHeader,
            "image/webp" => FormFileFactory.WebpHeader,
            _ => FormFileFactory.JpegHeader
        };

        Assert.Null(ImageFileValidator.Validate(FormFileFactory.Create(header, name, contentType)));
    }
}
