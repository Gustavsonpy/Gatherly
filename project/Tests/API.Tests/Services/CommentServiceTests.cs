
using API.DTO.Comment;
using API.Interfaces;
using API.Interfaces.Comment;
using API.Models;
using API.Services;
using Moq;

namespace API.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _userRepository = new();

    private CommentService CreateService() =>
        new(
            _commentRepository.Object,
            _currentUser.Object,
            _userRepository.Object);

    [Fact]
    public async Task CreateAsync_DeveCriarComentario_QuandoUsuarioExiste()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _currentUser.SetupGet(u => u.UserId).Returns(userId);

        _userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User
            {
                Id = userId,
                Name = "Maria",
                UrlPhoto = "foto.jpg"
            });

        _commentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        var result = await CreateService().createAsync(
            new CreateCommentDTO
            {
                Text = "Vou participar!",
                EventId = eventId
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("Vou participar!", result.Value!.Text);
        Assert.Equal(eventId, result.Value.EventId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("Maria", result.Value.CommentUserName);

        _commentRepository.Verify(
            r => r.AddAsync(It.Is<Comment>(c =>
                c.Text == "Vou participar!" &&
                c.EventId == eventId &&
                c.UserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoUsuarioNaoEstaAutenticado()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.Empty);

        var result = await CreateService().createAsync(
            new CreateCommentDTO
            {
                Text = "Comentário",
                EventId = Guid.NewGuid()
            });

        Assert.False(result.IsSuccess);
        Assert.Contains("Usuário não autenticado", result.Errors);

        _userRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoUsuarioNaoExiste()
    {
        var userId = Guid.NewGuid();

        _currentUser.SetupGet(u => u.UserId).Returns(userId);

        _userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var result = await CreateService().createAsync(
            new CreateCommentDTO
            {
                Text = "Comentário",
                EventId = Guid.NewGuid()
            });

        Assert.False(result.IsSuccess);
        Assert.Contains("Usuário não encontrado!", result.Errors);

        _commentRepository.Verify(
            r => r.AddAsync(It.IsAny<Comment>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_DeveMapearComentariosParaDTO()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _commentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Comment>
            {
                new()
                {
                    Text = "Ótimo evento",
                    EventId = eventId,
                    UserId = userId,
                    RegisterDate = DateTime.UtcNow,
                    User = new User
                    {
                        Id = userId,
                        Name = "João",
                        UrlPhoto = "joao.jpg"
                    }
                }
            });

        var result = await CreateService().GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Ótimo evento", result.Value[0].Text);
        Assert.Equal("João", result.Value[0].CommentUserName);
        Assert.Equal("joao.jpg", result.Value[0].UserPhoto);
    }

    [Fact]
    public async Task GetCommentsByEvent_DeveRetornarComentariosDoEvento()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _commentRepository.Setup(r => r.GetCommentsByEventAsync(eventId))
            .ReturnsAsync(new List<Comment>
            {
                new()
                {
                    Text = "Estarei lá",
                    EventId = eventId,
                    UserId = userId,
                    User = new User { Id = userId, Name = "Ana" }
                }
            });

        var result = await CreateService().getCommentsByEvent(eventId);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(eventId, result.Value[0].EventId);
        Assert.Equal("Ana", result.Value[0].CommentUserName);
    }

    [Fact]
    public async Task GetCommentsByEvent_DeveRetornarListaVazia_QuandoNaoHaComentarios()
    {
        var eventId = Guid.NewGuid();

        _commentRepository.Setup(r => r.GetCommentsByEventAsync(eventId))
            .ReturnsAsync(new List<Comment>());

        var result = await CreateService().getCommentsByEvent(eventId);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
