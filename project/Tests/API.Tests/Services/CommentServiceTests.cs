using API.DTO.Comment;
using API.Interfaces;
using API.Interfaces.Comment;
using API.Interfaces.Event;
using API.Models;
using API.Services;
using API.Tests.Helpers;
using Moq;

namespace API.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly User _user = TestData.User("Ana");
    private readonly Event _event = TestData.Event(Guid.NewGuid(), Guid.NewGuid());

    private CommentService CreateService(Guid? userId = null)
    {
        _currentUser.SetupGet(c => c.UserId).Returns(userId ?? _user.Id);
        return new CommentService(_comments.Object, _currentUser.Object, _users.Object, _events.Object);
    }

    private void SetupHappyPath()
    {
        _users.Setup(r => r.GetByIdAsync(_user.Id)).ReturnsAsync(_user);
        _events.Setup(r => r.GetByIdAsync(_event.Id)).ReturnsAsync(_event);
    }

    [Fact]
    public async Task CreateAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).createAsync(new CreateCommentDTO { Text = "oi" });
        Assert.Equal("Usuário não autenticado", result.Errors.Single());
    }

    [Fact]
    public async Task CreateAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().createAsync(new CreateCommentDTO { Text = "oi" });
        Assert.Equal("Usuário não encontrado!", result.Errors.Single());
    }

    [Fact]
    public async Task CreateAsync_TextoVazio_RetornaErro()
    {
        SetupHappyPath();
        var result = await CreateService().createAsync(new CreateCommentDTO { Text = "   ", EventId = _event.Id });
        Assert.Equal("O comentário não pode ser vazio.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateAsync_EventoInexistente_RetornaErro()
    {
        _users.Setup(r => r.GetByIdAsync(_user.Id)).ReturnsAsync(_user);
        var result = await CreateService().createAsync(new CreateCommentDTO { Text = "oi", EventId = Guid.NewGuid() });
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateAsync_EventoCancelado_RetornaErro()
    {
        SetupHappyPath();
        _event.CanceledAt = DateTime.UtcNow;

        var result = await CreateService().createAsync(new CreateCommentDTO { Text = "oi", EventId = _event.Id });

        Assert.Equal("Não é possível comentar em um evento cancelado.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateAsync_Valido_SalvaERetornaComDadosDoUsuario()
    {
        SetupHappyPath();
        _comments.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync((Comment c) => c);

        var result = await CreateService().createAsync(new CreateCommentDTO { Text = "Bora!", EventId = _event.Id });

        Assert.True(result.IsSuccess);
        Assert.Equal("Bora!", result.Value!.Text);
        Assert.Equal("Ana", result.Value.CommentUserName);
        Assert.Equal(_user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task GetAllAsync_MapeiaComentarios()
    {
        _comments.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Comment>
        {
            new() { Text = "a", User = _user, UserId = _user.Id }
        });

        var result = await CreateService().GetAllAsync();

        Assert.Equal("Ana", result.Value!.Single().CommentUserName);
    }

    [Fact]
    public async Task GetCommentsByEvent_MapeiaComentarios()
    {
        _comments.Setup(r => r.GetCommentsByEventAsync(_event.Id)).ReturnsAsync(new List<Comment>
        {
            new() { Text = "a", User = _user, EventId = _event.Id },
            new() { Text = "b", User = _user, EventId = _event.Id }
        });

        var result = await CreateService().GetCommentsByEvent(_event.Id);

        Assert.Equal(new[] { "a", "b" }, result.Value!.Select(c => c.Text));
    }

    [Fact]
    public async Task MetodosNaoImplementados_Lancam()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetByIdAsync(1));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.UpdateCommentAsync("x"));
    }
}
