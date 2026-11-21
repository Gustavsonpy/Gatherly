using API.DTO;
using API.Interfaces;
using API.Models;
using API.Services;
using API.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private UserService CreateService() => new(_repo.Object, NullLogger<UserService>.Instance);

    private static CreateUserDTO ValidCreateDto() => new()
    {
        Name = "Ana",
        Email = "  Ana@Email.com ",
        Password = "123456",
        Birthday = new DateTime(2000, 5, 10)
    };

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaUsuarioComSenhaHash()
    {
        User? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => saved = u).ReturnsAsync((User u) => u);

        var result = await CreateService().CreateAsync(ValidCreateDto());

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana", result.Value!.Name);
        Assert.NotEqual("123456", saved!.HashPassword);
        Assert.True(BCrypt.Net.BCrypt.Verify("123456", saved.HashPassword));
        Assert.Equal(DateTimeKind.Utc, saved.Birthday.Kind);
        Assert.False(saved.IsSuspended);
        _repo.Verify(r => r.GetByEmail("ana@email.com"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DadosInvalidos_RetornaErrosPorCampo()
    {
        _repo.Setup(r => r.GetByEmail("ana@email.com")).ReturnsAsync(TestData.User());
        var dto = ValidCreateDto();
        dto.Name = "  ";
        dto.Birthday = default;

        var result = await CreateService().CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(new[] { "name", "birthDate", "email" }, result.FieldErrors.Keys);
        Assert.Equal("Já existe uma conta com esse e-mail.", result.FieldErrors["email"].Single());
        Assert.Equal(3, result.Errors.Count);
        _repo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DataNoFuturo_RetornaErro()
    {
        var dto = ValidCreateDto();
        dto.Birthday = DateTime.Today.AddDays(1);

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal("A data não pode ultrapassar o dia de hoje", result.FieldErrors["birthDate"].Single());
    }

    [Fact]
    public async Task CreateAsync_MenorDe13Anos_RetornaErro()
    {
        var dto = ValidCreateDto();
        dto.Birthday = DateTime.Today.AddYears(-12);

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal("Você precisa ter pelo menos 13 anos.", result.FieldErrors["birthDate"].Single());
    }

    [Fact]
    public async Task GetMeAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().GetMeAsync(Guid.NewGuid());
        Assert.Equal("Usuário não encontrado", result.Errors.Single());
    }

    [Fact]
    public async Task GetMeAsync_RetornaDadosCompletos()
    {
        var user = TestData.User("Ana", "ana@email.com");
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().GetMeAsync(user.Id);

        Assert.Equal("ana@email.com", result.Value!.Email);
        Assert.Equal(user.Id, result.Value.Id);
    }

    [Fact]
    public async Task UpdateMeAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().UpdateMeAsync(Guid.NewGuid(), new UpdateUserDTO());
        Assert.Equal("Usuário não encontrado", result.Errors.Single());
    }

    [Fact]
    public async Task UpdateMeAsync_DadosInvalidos_RetornaErrosPorCampo()
    {
        var user = TestData.User();
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().UpdateMeAsync(user.Id, new UpdateUserDTO { Name = null!, Birthday = default });

        Assert.Equal(new[] { "name", "birthday" }, result.FieldErrors.Keys);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMeAsync_NascimentoNoFuturo_RetornaErro()
    {
        var user = TestData.User();
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().UpdateMeAsync(user.Id,
            new UpdateUserDTO { Name = "Ana", Birthday = DateTime.Today.AddDays(2) });

        Assert.Equal("A data de nascimento não pode ser no futuro.", result.FieldErrors["birthday"].Single());
    }

    [Fact]
    public async Task UpdateMeAsync_DadosValidos_NormalizaEAtualiza()
    {
        var user = TestData.User();
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().UpdateMeAsync(user.Id, new UpdateUserDTO
        {
            Name = "  Ana Maria ",
            Birthday = new DateTime(1999, 1, 1),
            Description = "   ",
            City = " Curitiba "
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana Maria", user.Name);
        Assert.Null(user.Description);
        Assert.Equal("Curitiba", user.City);
        _repo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdatePhotoAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().UpdatePhotoAsync(Guid.NewGuid(), "url");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdatePhotoAsync_AtualizaUrl()
    {
        var user = TestData.User();
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().UpdatePhotoAsync(user.Id, "http://x/uploads/users/a.jpg");

        Assert.Equal("http://x/uploads/users/a.jpg", result.Value!.UrlPhoto);
        _repo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().GetByIdAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaDadosPublicos()
    {
        var user = TestData.User("Ana");
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateService().GetByIdAsync(user.Id);

        Assert.Equal("Ana", result.Value!.Name);
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodos()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { TestData.User("A"), TestData.User("B") });

        var result = await CreateService().GetAllAsync();

        Assert.Equal(new[] { "A", "B" }, result.Value!.Select(u => u.Name));
    }

    [Fact]
    public async Task UpdateEmailAsync_NaoImplementado()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => CreateService().UpdateEmailAsync("x"));
    }
}
