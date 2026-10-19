
using API.Common;
using API.Controllers;
using API.DTO;
using API.DTO.User;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _service = new();

    private UserController CreateController() =>
        new(_service.Object);

    [Fact]
    public async Task Create_DeveRetornarOk_QuandoCadastroTemSucesso()
    {
        var user = new UserDTO
        {
            Name = "Maria",
            Email = "maria@example.com"
        };

        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserDTO>()))
            .ReturnsAsync(Result<UserDTO>.Success(user));

        var result = await CreateController().Create(new CreateUserDTO());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public async Task Create_DeveRetornarBadRequest_QuandoCadastroFalha()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateUserDTO>()))
            .ReturnsAsync(Result<UserDTO>.Failure("E-mail inválido"));

        var result = await CreateController().Create(new CreateUserDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOk_QuandoConsultaTemSucesso()
    {
        var users = new List<UserPublicDTO>
        {
            new() { Name = "Maria", City = "Joinville" },
            new() { Name = "João", City = "Curitiba" }
        };

        _service.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<List<UserPublicDTO>>.Success(users));

        var result = await CreateController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(users, ok.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarBadRequest_QuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();

        _service.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(
                Result<UserPublicDTO?>.Failure("Usuário não encontrado"));

        var result = await CreateController().GetById(id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoUsuarioExiste()
    {
        var id = Guid.NewGuid();
        var user = new UserPublicDTO { Id = id, Name = "Maria" };

        _service.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(Result<UserPublicDTO?>.Success(user));

        var result = await CreateController().GetById(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(user, ok.Value);
    }
}
