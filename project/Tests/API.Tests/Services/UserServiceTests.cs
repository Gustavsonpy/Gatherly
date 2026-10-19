
using API.DTO;
using API.Interfaces;
using API.Models;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ILogger<UserService>> _logger = new();

    private UserService CreateService() =>
        new(_userRepository.Object, _logger.Object);

    private static CreateUserDTO CreateValidUser() =>
        new()
        {
            Name = "Maria",
            Email = "maria@example.com",
            Password = "senha123",
            Birthday = DateTime.Today.AddYears(-21)
        };

    [Fact]
    public async Task CreateAsync_DeveCadastrarUsuario_QuandoDadosSaoValidos()
    {
        var dto = CreateValidUser();

        _userRepository
            .Setup(r => r.GetByEmail(dto.Email))
            .ReturnsAsync((User?)null);

        _userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        var service = CreateService();

        var result = await service.CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.Email, result.Value.Email);
        Assert.False(result.Value.IsSuspended);

        _userRepository.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Email == dto.Email &&
                u.Name == dto.Name &&
                !string.IsNullOrWhiteSpace(u.HashPassword))),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoEmailJaExiste()
    {
        var dto = CreateValidUser();

        _userRepository
            .Setup(r => r.GetByEmail(dto.Email))
            .ReturnsAsync(new User
            {
                Email = dto.Email,
                Name = "Outra pessoa"
            });

        var service = CreateService();

        var result = await service.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            result.Errors,
            error => error.Contains("Já existe uma conta"));

        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoSenhaTemMenosDeSeisCaracteres()
    {
        var dto = CreateValidUser();
        dto.Password = "12345";

        _userRepository
            .Setup(r => r.GetByEmail(dto.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            result.Errors,
            error => error.Contains("6 caracteres"));

        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoNascimentoEstaNoFuturo()
    {
        var dto = CreateValidUser();
        dto.Birthday = DateTime.Today.AddDays(1);

        _userRepository
            .Setup(r => r.GetByEmail(dto.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            result.Errors,
            error => error.Contains("data não pode ultrapassar"));

        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarPerfil_QuandoUsuarioExiste()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User
            {
                Id = userId,
                Name = "Maria",
                Description = "Gosto de vôlei",
                City = "Joinville",
                UrlPhoto = "https://example.com/foto.jpg"
            });

        var service = CreateService();

        var result = await service.GetByIdAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.Id);
        Assert.Equal("Maria", result.Value.Name);
        Assert.Equal("Joinville", result.Value.City);
        Assert.Equal("Gosto de vôlei", result.Value.Description);
    }

    [Fact]
    public async Task GetByIdAsync_DeveFalhar_QuandoUsuarioNaoExiste()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.GetByIdAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Usuário não encontrado", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarPerfisPublicos()
    {
        _userRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria",
                    City = "Joinville"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "João",
                    City = "Curitiba"
                }
            });

        var service = CreateService();

        var result = await service.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, u => u.Name == "Maria");
        Assert.Contains(result.Value, u => u.Name == "João");
    }
}
