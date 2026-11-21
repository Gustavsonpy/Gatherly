using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Category;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Models;
using API.Services;
using API.Tests.Helpers;
using Moq;
using static API.Interfaces.Participant.IEventParticipantRepository;

namespace API.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IEventParticipantRepository> _participants = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    private EventService CreateService(Guid? currentUserId = null)
    {
        _currentUser.SetupGet(c => c.UserId).Returns(currentUserId ?? _userId);
        return new EventService(_events.Object, _users.Object, _participants.Object, _currentUser.Object, _categories.Object);
    }

    private CreateEventDTO ValidCreateDto() => new()
    {
        Title = "Futebol",
        Description = "Partida",
        DateTimeStart = DateTime.UtcNow.AddDays(1),
        DateTimeEnd = DateTime.UtcNow.AddDays(1).AddHours(2),
        Localization = "Parque",
        MaxCapacity = 10,
        City = "Curitiba",
        UrlImage = "http://localhost:5008/uploads/events/a.jpg",
        CategoryId = _categoryId
    };

    private Event FutureEvent(Guid? ownerId = null) =>
        TestData.Event(ownerId ?? Guid.NewGuid(), _categoryId);

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_DadosValidos_CriaEventoComOrganizadorComoParticipante()
    {
        _categories.Setup(r => r.GetByIdAsync(_categoryId)).ReturnsAsync(TestData.Category());
        Event? saved = null;
        _events.Setup(r => r.AddAsync(It.IsAny<Event>()))
            .Callback<Event>(e => saved = e)
            .ReturnsAsync((Event e) => e);

        var result = await CreateService().CreateAsync(ValidCreateDto(), _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Futebol", result.Value!.Title);
        Assert.Equal(_userId, result.Value.UserId);
        var participant = Assert.Single(saved!.Participants);
        Assert.Equal(_userId, participant.UserId);
    }

    [Fact]
    public async Task CreateAsync_DadosInvalidos_RetornaTodosOsErros()
    {
        var dto = ValidCreateDto();
        dto.Title = "   ";
        dto.DateTimeStart = DateTime.UtcNow.AddHours(-1);
        dto.DateTimeEnd = dto.DateTimeStart.AddHours(-1);
        dto.UrlImage = "https://site-malicioso.com/foto.jpg";

        var result = await CreateService().CreateAsync(dto, _userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(new[]
        {
            "O título é obrigatório.",
            "O evento deve começar no futuro.",
            "O término deve ser depois do início",
            "Imagem inválida.",
            "Categoria não encontrada."
        }, result.Errors);
        _events.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Theory]
    [InlineData("nao-e-url")]
    [InlineData("ftp://localhost/uploads/events/a.jpg")]
    [InlineData("http://localhost/uploads/users/a.jpg")]
    public async Task CreateAsync_UrlDeImagemInvalida_RetornaErro(string url)
    {
        _categories.Setup(r => r.GetByIdAsync(_categoryId)).ReturnsAsync(TestData.Category());
        var dto = ValidCreateDto();
        dto.UrlImage = url;

        var result = await CreateService().CreateAsync(dto, _userId);

        Assert.Equal(new[] { "Imagem inválida." }, result.Errors);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetByIdAsync_EventoExistente_RetornaDtoComContagemEParticipacao()
    {
        var ev = FutureEvent();
        ev.Category = new Category { Name = "Esportes" };
        ev.User = TestData.User("Criador");
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _participants.Setup(r => r.CountByEventAsync(ev.Id)).ReturnsAsync(3);
        _participants.Setup(r => r.ExistsAsync(ev.Id, _userId)).ReturnsAsync(true);

        var result = await CreateService().GetByIdAsync(ev.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.ParticipantsCount);
        Assert.True(result.Value.IsParticipating);
        Assert.Equal("Esportes", result.Value.CategoryName);
        Assert.Equal("Criador", result.Value.CreatorName);
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioAnonimo_NaoConsultaParticipacao()
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService(Guid.Empty).GetByIdAsync(ev.Id);

        Assert.False(result.Value!.IsParticipating);
        _participants.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    // ---------- Listagens / status ----------

    [Fact]
    public async Task GetAllAsync_CalculaStatusEParticipantesAtivos()
    {
        var now = DateTime.UtcNow;
        var upcoming = FutureEvent();
        upcoming.Participants.Add(TestData.Participant(upcoming.Id, _userId));
        upcoming.Participants.Add(TestData.Participant(upcoming.Id, Guid.NewGuid(), leftAt: now));
        var ongoing = TestData.Event(Guid.NewGuid(), _categoryId, now.AddHours(-1), now.AddHours(1));
        var done = TestData.Event(Guid.NewGuid(), _categoryId, now.AddDays(-2), now.AddDays(-1));
        var canceled = TestData.Event(Guid.NewGuid(), _categoryId, canceledAt: now);
        _events.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Event> { upcoming, ongoing, done, canceled });

        var result = await CreateService().GetAllAsync();

        Assert.Equal(new[] { "upcoming", "ongoing", "done", "canceled" }, result.Value!.Select(e => e.Status));
        Assert.Equal(1, result.Value![0].ParticipantsCount);
        Assert.True(result.Value[0].IsParticipating);
        Assert.False(result.Value[1].IsParticipating);
    }

    [Fact]
    public async Task GetMyEventsAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).GetMyEventsAsync();
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetMyEventsAsync_RetornaEventosDoUsuario()
    {
        _participants.Setup(r => r.GetEventsByActiveUser(_userId)).ReturnsAsync(new List<Event> { FutureEvent() });

        var result = await CreateService().GetMyEventsAsync();

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetMyCityEventsAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).GetMyCityEventsAsync();
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetMyCityEventsAsync_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().GetMyCityEventsAsync();
        Assert.Equal("Usuário não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetMyCityEventsAsync_UsuarioSemCidade_RetornaErro()
    {
        _users.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(TestData.User(city: null));

        var result = await CreateService().GetMyCityEventsAsync();

        Assert.Equal("Usuário não possui cidade cadastrada.", result.Errors.Single());
    }

    [Fact]
    public async Task GetMyCityEventsAsync_RetornaEventosDaCidadeDoUsuario()
    {
        _users.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(TestData.User(city: "Maringá"));
        _events.Setup(r => r.GetByCityAsync("Maringá")).ReturnsAsync(new List<Event> { FutureEvent() });

        var result = await CreateService().GetMyCityEventsAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetFutureEvents_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).GetFutureEvents();
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetFutureEvents_UsuarioInexistente_RetornaErro()
    {
        var result = await CreateService().GetFutureEvents();
        Assert.Equal("Usuário não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task GetFutureEvents_RetornaEventosFuturos()
    {
        _users.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(TestData.User());
        _events.Setup(r => r.GetFutureEvents()).ReturnsAsync(new List<Event> { FutureEvent(), FutureEvent() });

        var result = await CreateService().GetFutureEvents();

        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetParticipantsAsync_RetornaParticipantesDoRepositorio()
    {
        var eventId = Guid.NewGuid();
        var list = new List<ParticipantDTO> { new() { Id = _userId, Name = "Ana" } };
        _participants.Setup(r => r.GetParticipantsByEventAsync(eventId)).ReturnsAsync(list);

        var result = await CreateService().GetParticipantsAsync(eventId);

        Assert.Same(list, result.Value);
    }

    [Fact]
    public async Task UpdateTitleAsync_NaoImplementado()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => CreateService().UpdateTitleAsync("x"));
    }

    // ---------- JoinAsync ----------

    [Fact]
    public async Task JoinAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).JoinAsync(Guid.NewGuid());
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().JoinAsync(Guid.NewGuid());
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_EventoCancelado_RetornaErro()
    {
        var ev = FutureEvent();
        ev.CanceledAt = DateTime.UtcNow;
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal("Este evento foi cancelado.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_EventoEmAndamento_RetornaErro()
    {
        var ev = TestData.Event(Guid.NewGuid(), _categoryId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal("Este evento já está em andamento.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_EventoEncerrado_RetornaErro()
    {
        var ev = TestData.Event(Guid.NewGuid(), _categoryId, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal("Este evento já aconteceu.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_JaParticipa_RetornaErro()
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _participants.Setup(r => r.ExistsAsync(ev.Id, _userId)).ReturnsAsync(true);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal("Você já participa deste evento.", result.Errors.Single());
    }

    [Fact]
    public async Task JoinAsync_EventoLotado_RetornaErro()
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _participants.Setup(r => r.CountByEventAsync(ev.Id)).ReturnsAsync(ev.MaxCapacity);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal("Evento lotado.", result.Errors.Single());
        _participants.Verify(r => r.JoinAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Theory]
    [InlineData(JoinEventStatus.Joined, true, null)]
    [InlineData(JoinEventStatus.AlreadyJoined, false, "Você já participa deste evento.")]
    [InlineData(JoinEventStatus.Full, false, "Evento lotado.")]
    [InlineData(JoinEventStatus.EventNotFound, false, "Evento não encontrado.")]
    public async Task JoinAsync_TraduzStatusDoRepositorio(JoinEventStatus status, bool success, string? error)
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _participants.Setup(r => r.JoinAsync(ev.Id, _userId)).ReturnsAsync(status);

        var result = await CreateService().JoinAsync(ev.Id);

        Assert.Equal(success, result.IsSuccess);
        if (error is not null) Assert.Equal(error, result.Errors.Single());
    }

    // ---------- LeaveAsync ----------

    [Fact]
    public async Task LeaveAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).LeaveAsync(Guid.NewGuid());
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task LeaveAsync_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().LeaveAsync(Guid.NewGuid());
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task LeaveAsync_Organizador_NaoPodeSair()
    {
        var ev = FutureEvent(_userId);
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().LeaveAsync(ev.Id);

        Assert.Equal("O organizador não pode sair do próprio evento.", result.Errors.Single());
    }

    [Fact]
    public async Task LeaveAsync_EventoJaComecou_RetornaErro()
    {
        var ev = TestData.Event(Guid.NewGuid(), _categoryId, DateTime.UtcNow.AddMinutes(-5));
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().LeaveAsync(ev.Id);

        Assert.Equal("Não é possível sair de um evento que já começou.", result.Errors.Single());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LeaveAsync_RetornaResultadoDaRemocao(bool removed)
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);
        _participants.Setup(r => r.RemoveAsync(ev.Id, _userId)).ReturnsAsync(removed);

        var result = await CreateService().LeaveAsync(ev.Id);

        Assert.Equal(removed, result.IsSuccess);
        if (!removed) Assert.Equal("Você não participa deste evento.", result.Errors.Single());
    }

    // ---------- CancelAsync ----------

    [Fact]
    public async Task CancelAsync_NaoAutenticado_RetornaErro()
    {
        var result = await CreateService(Guid.Empty).CancelAsync(Guid.NewGuid());
        Assert.Equal("Usuário não autenticado.", result.Errors.Single());
    }

    [Fact]
    public async Task CancelAsync_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().CancelAsync(Guid.NewGuid());
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task CancelAsync_NaoOrganizador_RetornaErro()
    {
        var ev = FutureEvent();
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().CancelAsync(ev.Id);

        Assert.Equal("Apenas o organizador pode cancelar o evento.", result.Errors.Single());
    }

    [Fact]
    public async Task CancelAsync_JaCancelado_RetornaErro()
    {
        var ev = FutureEvent(_userId);
        ev.CanceledAt = DateTime.UtcNow;
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().CancelAsync(ev.Id);

        Assert.Equal("Este evento já foi cancelado.", result.Errors.Single());
    }

    [Fact]
    public async Task CancelAsync_EventoJaComecou_RetornaErro()
    {
        var ev = TestData.Event(_userId, _categoryId, DateTime.UtcNow.AddMinutes(-1));
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().CancelAsync(ev.Id);

        Assert.Equal("Só é possível cancelar eventos que ainda não começaram.", result.Errors.Single());
    }

    [Fact]
    public async Task CancelAsync_Valido_MarcaCanceladoESalva()
    {
        var ev = FutureEvent(_userId);
        _events.Setup(r => r.GetByIdAsync(ev.Id)).ReturnsAsync(ev);

        var result = await CreateService().CancelAsync(ev.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(ev.CanceledAt);
        _events.Verify(r => r.UpdateAsync(ev), Times.Once);
    }
}
