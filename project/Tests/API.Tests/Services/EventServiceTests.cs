
using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Models;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEventParticipantRepository> _participantRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<ILogger<EventService>> _logger = new();

    private EventService CreateService() =>
        new(
            _eventRepository.Object,
            _userRepository.Object,
            _participantRepository.Object,
            _currentUserService.Object,
            _logger.Object
        );

    [Fact]
    public async Task CreateAsync_DeveCriarEvento_QuandoTituloNaoExiste()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var dto = new CreateEventDTO
        {
            Title = "Futebol de domingo",
            Description = "Partida entre amigos",
            DateTime = DateTime.UtcNow.AddDays(3),
            Localization = "Ginásio municipal",
            MaxCapacity = 10,
            City = "Joinville",
            Level = "Iniciante",
            UrlImage = "https://exemplo.com/futebol.jpg",
            CategoryId = categoryId
        };

        _eventRepository
            .Setup(r => r.GetByTitle(dto.Title))
            .ReturnsAsync((Event?)null);

        _eventRepository
            .Setup(r => r.AddAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => e);

        var service = CreateService();

        var result = await service.CreateAsync(dto, userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Title, result.Value.Title);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(categoryId, result.Value.CategoryId);

        _eventRepository.Verify(
            r => r.AddAsync(It.Is<Event>(e =>
                e.Title == dto.Title &&
                e.UserId == userId &&
                e.CategoryId == categoryId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeveFalhar_QuandoTituloJaExiste()
    {
        var dto = new CreateEventDTO
        {
            Title = "Futebol",
            Description = "Partida",
            DateTime = DateTime.UtcNow.AddDays(2),
            Localization = "Quadra",
            MaxCapacity = 10,
            City = "Joinville",
            UrlImage = "",
            CategoryId = Guid.NewGuid()
        };

        _eventRepository
            .Setup(r => r.GetByTitle(dto.Title))
            .ReturnsAsync(new Event
            {
                Title = dto.Title
            });

        var service = CreateService();

        var result = await service.CreateAsync(dto, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains(
            result.Errors,
            error => error.Contains("Already exists"));

        _eventRepository.Verify(
            r => r.AddAsync(It.IsAny<Event>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_DeveFalhar_QuandoEventoNaoExiste()
    {
        var eventId = Guid.NewGuid();

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var service = CreateService();

        var result = await service.GetByIdAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Evento não encontrado.", result.Errors);
    }

    [Fact]
    public async Task GetMyCityEventsAsync_DeveFalhar_QuandoUsuarioNaoEstaAutenticado()
    {
        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(Guid.Empty);

        var service = CreateService();

        var result = await service.GetMyCityEventsAsync();

        Assert.False(result.IsSuccess);
        Assert.Contains("Usuário não autenticado.", result.Errors);

        _userRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyCityEventsAsync_DeveBuscarEventosDaCidadeDoUsuario()
    {
        var userId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _userRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User
            {
                Id = userId,
                Name = "Usuário",
                City = "Joinville"
            });

        _eventRepository
            .Setup(r => r.GetByCityAsync("Joinville"))
            .ReturnsAsync(new List<Event>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Vôlei",
                    Description = "Partida",
                    DateTime = new DateTime(2027, 1, 10, 15, 30, 0),
                    Localization = "Quadra central",
                    MaxCapacity = 12,
                    City = "Joinville",
                    UrlImage = "",
                    UserId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid()
                }
            });

        var service = CreateService();

        var result = await service.GetMyCityEventsAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Vôlei", result.Value[0].Title);
        Assert.Equal("Joinville", result.Value[0].City);
        Assert.Equal("15:30", result.Value[0].Time);

        _eventRepository.Verify(
            r => r.GetByCityAsync("Joinville"),
            Times.Once);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoUsuarioNaoEstaAutenticado()
    {
        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(Guid.Empty);

        var service = CreateService();

        var result = await service.JoinAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Usuário não autenticado.", result.Errors);

        _eventRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoUsuarioEhOrganizador()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                UserId = userId,
                DateTime = DateTime.UtcNow.AddDays(2),
                MaxCapacity = 10
            });

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            "Você é o organizador deste evento.",
            result.Errors);

        _participantRepository.Verify(
            r => r.AddAsync(It.IsAny<EventParticipant>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoEventoEstaLotado()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                UserId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow.AddDays(2),
                MaxCapacity = 2
            });

        _participantRepository
            .Setup(r => r.ExistsAsync(eventId, userId))
            .ReturnsAsync(false);

        _participantRepository
            .Setup(r => r.CountByEventAsync(eventId))
            .ReturnsAsync(2);

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Evento lotado.", result.Errors);

        _participantRepository.Verify(
            r => r.AddAsync(It.IsAny<EventParticipant>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinAsync_DeveAdicionarParticipante_QuandoHaVaga()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                UserId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow.AddDays(2),
                MaxCapacity = 10
            });

        _participantRepository
            .Setup(r => r.ExistsAsync(eventId, userId))
            .ReturnsAsync(false);

        _participantRepository
            .Setup(r => r.CountByEventAsync(eventId))
            .ReturnsAsync(3);

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        _participantRepository.Verify(
            r => r.AddAsync(It.Is<EventParticipant>(p =>
                p.EventId == eventId &&
                p.UserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task LeaveAsync_DeveFalhar_QuandoUsuarioNaoParticipa()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _participantRepository
            .Setup(r => r.RemoveAsync(eventId, userId))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.LeaveAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            "Você não participa deste evento.",
            result.Errors);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoEventoNaoExiste()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Evento não encontrado.", result.Errors);

        _participantRepository.Verify(
            r => r.AddAsync(It.IsAny<EventParticipant>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoEventoJaAconteceu()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                UserId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow.AddDays(-1),
                MaxCapacity = 10
            });

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Este evento já aconteceu.", result.Errors);

        _participantRepository.Verify(
            r => r.AddAsync(It.IsAny<EventParticipant>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinAsync_DeveFalhar_QuandoUsuarioJaParticipa()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _eventRepository
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new Event
            {
                Id = eventId,
                UserId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow.AddDays(2),
                MaxCapacity = 10
            });

        _participantRepository
            .Setup(r => r.ExistsAsync(eventId, userId))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.JoinAsync(eventId);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            "Você já participa deste evento.",
            result.Errors);

        _participantRepository.Verify(
            r => r.AddAsync(It.IsAny<EventParticipant>()),
            Times.Never);
    }

    [Fact]
    public async Task LeaveAsync_DeveRemoverParticipacao_QuandoUsuarioParticipa()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserService
            .SetupGet(s => s.UserId)
            .Returns(userId);

        _participantRepository
            .Setup(r => r.RemoveAsync(eventId, userId))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.LeaveAsync(eventId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        _participantRepository.Verify(
            r => r.RemoveAsync(eventId, userId),
            Times.Once);
    }

    [Fact]
    public async Task GetParticipantsAsync_DeveRetornarDadosPublicosDosParticipantes()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _participantRepository
            .Setup(r => r.GetUsersByEventAsync(eventId))
            .ReturnsAsync(new List<User>
            {
                new()
                {
                    Id = userId,
                    Name = "Maria",
                    UrlPhoto = "https://example.com/maria.jpg"
                }
            });

        var service = CreateService();

        var result = await service.GetParticipantsAsync(eventId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(userId, result.Value[0].Id);
        Assert.Equal("Maria", result.Value[0].Name);
        Assert.Equal(
            "https://example.com/maria.jpg",
            result.Value[0].UrlPhoto);
    }
}
