using API.DTO.Rate;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Interfaces.Rate;
using API.Models;
using API.Services;
using API.Tests.Helpers;
using Moq;

namespace API.Tests.Services;

public class UserRatingServiceTests
{
    private readonly Mock<IUserRatingRepository> _ratings = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventParticipantRepository> _participants = new();
    private readonly Guid _reporter = Guid.NewGuid();
    private readonly Guid _rated = Guid.NewGuid();
    private readonly Event _pastEvent;

    public UserRatingServiceTests()
    {
        _pastEvent = TestData.Event(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
    }

    private UserRatingService CreateService() => new(_ratings.Object, _events.Object, _participants.Object);

    private CreateUserRatingDTO Dto(int rate = 5) => new()
    {
        Rate = rate, RatedUserId = _rated, EventId = _pastEvent.Id, Description = "Top"
    };

    private void SetupAttended(bool reporter = true, bool rated = true)
    {
        _events.Setup(r => r.GetByIdAsync(_pastEvent.Id)).ReturnsAsync(_pastEvent);
        _participants.Setup(r => r.AttendedAsync(_pastEvent.Id, _reporter, _pastEvent.DateTimeStart)).ReturnsAsync(reporter);
        _participants.Setup(r => r.AttendedAsync(_pastEvent.Id, _rated, _pastEvent.DateTimeStart)).ReturnsAsync(rated);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateRatingAsync_NotaForaDoIntervalo_RetornaErro(int rate)
    {
        var result = await CreateService().CreateRatingAsync(_reporter, Dto(rate));
        Assert.Equal("A avaliação tem que ser entre 1 e 5 estrelas.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_AutoAvaliacao_RetornaErro()
    {
        var dto = Dto();
        dto.RatedUserId = _reporter;

        var result = await CreateService().CreateRatingAsync(_reporter, dto);

        Assert.Equal("Você não pode avaliar a si mesmo.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().CreateRatingAsync(_reporter, Dto());
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_EventoCancelado_RetornaErro()
    {
        SetupAttended();
        _pastEvent.CanceledAt = DateTime.UtcNow;

        var result = await CreateService().CreateRatingAsync(_reporter, Dto());

        Assert.Equal("Não é possível avaliar participantes de um evento cancelado.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_EventoNaoTerminou_RetornaErro()
    {
        SetupAttended();
        _pastEvent.DateTimeEnd = DateTime.UtcNow.AddHours(1);

        var result = await CreateService().CreateRatingAsync(_reporter, Dto());

        Assert.Contains("depois que o evento terminar", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_AvaliadorNaoParticipou_RetornaErro()
    {
        SetupAttended(reporter: false);
        var result = await CreateService().CreateRatingAsync(_reporter, Dto());
        Assert.Equal("Você não participou desse evento.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_AvaliadoNaoParticipou_RetornaErro()
    {
        SetupAttended(rated: false);
        var result = await CreateService().CreateRatingAsync(_reporter, Dto());
        Assert.Equal("Este usuário não participou desse evento.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateRatingAsync_Valida_SalvaAvaliacao()
    {
        SetupAttended();
        _ratings.Setup(r => r.AddAsync(It.IsAny<UserRating>())).ReturnsAsync((UserRating r) => r);

        var result = await CreateService().CreateRatingAsync(_reporter, Dto(4));

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Rate);
        Assert.Equal(_rated, result.Value.RatedUserId);
    }

    [Fact]
    public async Task GetRatingByUser_OrdenaDoMaisRecente()
    {
        _ratings.Setup(r => r.GetRateByUser(_rated)).ReturnsAsync(new List<UserRating>
        {
            new() { Rate = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Rate = 5, CreatedAt = DateTime.UtcNow }
        });

        var result = await CreateService().GetRatingByUser(_rated);

        Assert.Equal(new[] { 5, 1 }, result.Select(r => r.Rate));
    }

    [Fact]
    public async Task GetRatingsReceivedByUser_RetornaAvaliacoes()
    {
        _ratings.Setup(r => r.GetRateByUser(_rated)).ReturnsAsync(new List<UserRating> { new() { Rate = 3 } });

        var result = await CreateService().GetRatingsReceivedByUser(_rated);

        Assert.Equal(3, result.Single().Rate);
    }

    [Fact]
    public async Task GetUserScore_RetornaMediaEContagem()
    {
        _ratings.Setup(r => r.GetUserScoreAsync(_rated)).ReturnsAsync((4.5, 2));

        var result = await CreateService().GetUserScore(_rated);

        Assert.Equal(_rated, result.UserId);
        Assert.Equal(4.5, result.Average);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task MetodosNaoImplementados_Lancam()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetAll());
        await Assert.ThrowsAsync<NotImplementedException>(() => service.UpdateUserRating(new UserRatingDTO()));
    }
}
