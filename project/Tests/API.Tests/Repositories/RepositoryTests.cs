using API.Data;
using API.Enums;
using API.Models;
using API.Repositories;
using API.Tests.Helpers;

namespace API.Tests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly AppDbContext Db = TestDb.Create();
    protected readonly User Owner;
    protected readonly User Other;
    protected readonly Category Category;
    protected readonly Event Event;

    protected RepositoryTestBase()
    {
        Owner = TestData.User("Organizador");
        Other = TestData.User("Convidado");
        Category = TestData.Category();
        Db.AddRange(Owner, Other, Category);
        Event = TestData.Event(Owner.Id, Category.Id);
        Db.Events.Add(Event);
        Db.SaveChanges();
    }

    public void Dispose()
    {
        Db.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class UserRepositoryTests : RepositoryTestBase
{
    private static readonly string[] ExpectedUserNames = { "Convidado", "Organizador" };

    private UserRepository Repo => new(Db);

    [Fact]
    public async Task AddAsync_E_GetById_PersisteUsuario()
    {
        var user = await Repo.AddAsync(TestData.User("Novo", "novo@email.com"));

        var loaded = await Repo.GetByIdAsync(user.Id);

        Assert.Equal("novo@email.com", loaded!.Email);
    }

    [Fact]
    public async Task GetByEmail_EncontraOuRetornaNulo()
    {
        Assert.Equal(Owner.Id, (await Repo.GetByEmail(Owner.Email))!.Id);
        Assert.Null(await Repo.GetByEmail("naoexiste@email.com"));
    }

    [Fact]
    public async Task GetAllAsync_OrdenaPorNome()
    {
        var names = (await Repo.GetAllAsync()).Select(u => u.Name);
        Assert.Equal(ExpectedUserNames, names);
    }

    [Fact]
    public async Task UpdateAsync_SalvaAlteracoes()
    {
        Owner.City = "Londrina";
        await Repo.UpdateAsync(Owner);

        Assert.Equal("Londrina", (await Repo.GetByIdAsync(Owner.Id))!.City);
    }

    [Fact]
    public async Task MetodosNaoImplementados_Lancam()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => Repo.DeleteAsync(1));
        await Assert.ThrowsAsync<NotImplementedException>(() => Repo.GetActiveUsersAsync());
    }
}

public class CategoryRepositoryTests : RepositoryTestBase
{
    private static readonly string[] ExpectedCategoryNames = { "Esportes", "Música" };

    private CategoryRepository Repo => new(Db);

    [Fact]
    public async Task CrudCompleto()
    {
        var music = await Repo.AddAsync(TestData.Category("Música"));

        Assert.Equal(ExpectedCategoryNames, (await Repo.GetAllAsync()).Select(c => c.Name));
        Assert.Equal(music.Id, (await Repo.GetByName("Música"))!.Id);
        Assert.NotNull(await Repo.GetByIdAsync(music.Id));

        music.Name = "Shows";
        await Repo.UpdateAsync(music);
        Assert.NotNull(await Repo.GetByName("Shows"));

        await Repo.DeleteAsync(music.Id);
        Assert.Null(await Repo.GetByIdAsync(music.Id));
    }

    [Fact]
    public async Task DeleteAsync_IdInexistente_NaoLanca()
    {
        await Repo.DeleteAsync(Guid.NewGuid());
        Assert.Single(await Repo.GetAllAsync());
    }
}

public class EventRepositoryTests : RepositoryTestBase
{
    private static readonly string[] ExpectedEventTitles = { "Basquete", "Futebol" };

    private EventRepository Repo => new(Db);

    [Fact]
    public async Task GetByIdAsync_IncluiCategoriaECriador()
    {
        var ev = await Repo.GetByIdAsync(Event.Id);

        Assert.Equal("Esportes", ev!.Category.Name);
        Assert.Equal("Organizador", ev.User.Name);
    }

    [Fact]
    public async Task AddAsync_E_GetByTitle()
    {
        await Repo.AddAsync(TestData.Event(Owner.Id, Category.Id, title: "Vôlei"));

        Assert.NotNull(await Repo.GetByTitle("Vôlei"));
        Assert.Null(await Repo.GetByTitle("Inexistente"));
    }

    [Fact]
    public async Task GetAllAsync_OrdenaPorTitulo()
    {
        await Repo.AddAsync(TestData.Event(Owner.Id, Category.Id, title: "Basquete"));

        Assert.Equal(ExpectedEventTitles, (await Repo.GetAllAsync()).Select(e => e.Title));
    }

    [Fact]
    public async Task GetByCityAsync_FiltraPorCidade()
    {
        await Repo.AddAsync(TestData.Event(Owner.Id, Category.Id, city: "Maringá"));

        var result = await Repo.GetByCityAsync("Maringá");

        Assert.Equal("Maringá", Assert.Single(result).City);
    }

    [Fact]
    public async Task GetByUserAsync_RetornaEventosCriadosOuParticipados()
    {
        var other = TestData.Event(Other.Id, Category.Id, title: "Do convidado");
        var joined = TestData.Event(Other.Id, Category.Id, title: "Participando");
        joined.Participants.Add(TestData.Participant(joined.Id, Owner.Id));
        Db.Events.AddRange(other, joined);
        await Db.SaveChangesAsync();

        var titles = (await Repo.GetByUserAsync(Owner.Id)).Select(e => e.Title).ToList();

        Assert.Contains("Futebol", titles);
        Assert.Contains("Participando", titles);
        Assert.DoesNotContain("Do convidado", titles);
    }

    [Fact]
    public async Task GetFutureEvents_IgnoraPassadosECancelados()
    {
        Db.Events.AddRange(
            TestData.Event(Owner.Id, Category.Id, DateTime.UtcNow.AddDays(-1), title: "Passado"),
            TestData.Event(Owner.Id, Category.Id, title: "Cancelado", canceledAt: DateTime.UtcNow));
        await Db.SaveChangesAsync();

        var result = await Repo.GetFutureEvents();

        Assert.Equal("Futebol", Assert.Single(result).Title);
    }

    [Fact]
    public async Task UpdateAsync_SalvaAlteracoes()
    {
        Event.CanceledAt = DateTime.UtcNow;
        await Repo.UpdateAsync(Event);

        Assert.NotNull((await Repo.GetByIdAsync(Event.Id))!.CanceledAt);
    }

    [Fact]
    public async Task DeleteAsync_RemoveEvento()
    {
        await Repo.DeleteAsync(Event);

        Assert.Null(await Repo.GetByIdAsync(Event.Id));
    }

    [Fact]
    public async Task HasReportsOrRatingsAsync_DetectaDenunciaOuAvaliacao()
    {
        Assert.False(await Repo.HasReportsOrRatingsAsync(Event.Id));

        Db.UserReports.Add(new UserReport { ReporterId = Other.Id, EventId = Event.Id, Reason = API.Enums.ReportReason.Spam });
        await Db.SaveChangesAsync();

        Assert.True(await Repo.HasReportsOrRatingsAsync(Event.Id));
    }
}

public class EventParticipantRepositoryTests : RepositoryTestBase
{
    private static readonly string[] ExpectedParticipantNames = { "Organizador", "Convidado" };

    private EventParticipantRepository Repo => new(Db);

    private async Task AddParticipant(Guid userId, DateTime? leftAt = null, DateTime? registerDate = null)
    {
        Db.EventParticipants.Add(TestData.Participant(Event.Id, userId, leftAt, registerDate));
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task ExistsECount_ConsideramApenasParticipantesAtivos()
    {
        await AddParticipant(Owner.Id);
        await AddParticipant(Other.Id, leftAt: DateTime.UtcNow);

        Assert.True(await Repo.ExistsAsync(Event.Id, Owner.Id));
        Assert.False(await Repo.ExistsAsync(Event.Id, Other.Id));
        Assert.Equal(1, await Repo.CountByEventAsync(Event.Id));
    }

    [Fact]
    public async Task GetParticipantsByEventAsync_RetornaAtivosComNota()
    {
        var third = TestData.User("Terceiro");
        Db.Users.Add(third);
        await AddParticipant(Owner.Id, registerDate: DateTime.UtcNow.AddMinutes(-10));
        await AddParticipant(Other.Id, registerDate: DateTime.UtcNow);
        await AddParticipant(third.Id, leftAt: DateTime.UtcNow);
        Db.UserRatings.AddRange(
            new UserRating { RatedUserId = Owner.Id, ReporterId = Other.Id, EventId = Event.Id, Rate = 4 },
            new UserRating { RatedUserId = Owner.Id, ReporterId = third.Id, EventId = Event.Id, Rate = 5 });
        await Db.SaveChangesAsync();

        var result = await Repo.GetParticipantsByEventAsync(Event.Id);

        Assert.Equal(ExpectedParticipantNames, result.Select(p => p.Name));
        Assert.Equal(4.5, result[0].UserScore);
        Assert.Equal(2, result[0].RatingsCount);
        Assert.Equal(0, result[1].UserScore);
    }

    [Fact]
    public async Task GetEventsByActiveUser_IgnoraEventosQueSaiu()
    {
        var left = TestData.Event(Owner.Id, Category.Id, title: "Saí");
        Db.Events.Add(left);
        await AddParticipant(Other.Id);
        Db.EventParticipants.Add(TestData.Participant(left.Id, Other.Id, leftAt: DateTime.UtcNow));
        await Db.SaveChangesAsync();

        var result = await Repo.GetEventsByActiveUser(Other.Id);

        Assert.Equal("Futebol", Assert.Single(result).Title);
    }

    [Fact]
    public async Task RemoveAsync_MarcaSaida()
    {
        await AddParticipant(Other.Id);

        Assert.True(await Repo.RemoveAsync(Event.Id, Other.Id));
        Assert.False(await Repo.ExistsAsync(Event.Id, Other.Id));
        Assert.False(await Repo.RemoveAsync(Event.Id, Other.Id));
    }

    [Fact]
    public async Task AttendedAsync_ConsideraQuemSaiuDepoisDoInicio()
    {
        await AddParticipant(Owner.Id);
        await AddParticipant(Other.Id, leftAt: Event.DateTimeStart.AddMinutes(30));

        Assert.True(await Repo.AttendedAsync(Event.Id, Owner.Id, Event.DateTimeStart));
        Assert.True(await Repo.AttendedAsync(Event.Id, Other.Id, Event.DateTimeStart));
        Assert.False(await Repo.AttendedAsync(Event.Id, Other.Id, Event.DateTimeStart.AddHours(1)));
        Assert.False(await Repo.AttendedAsync(Event.Id, Guid.NewGuid(), Event.DateTimeStart));
    }

    [Fact]
    public async Task HasJoinedAsync_ConsideraQualquerParticipacao()
    {
        await AddParticipant(Other.Id, leftAt: DateTime.UtcNow);

        Assert.True(await Repo.HasJoinedAsync(Event.Id, Other.Id));
        Assert.False(await Repo.HasJoinedAsync(Event.Id, Guid.NewGuid()));
    }
}

public class CommentRepositoryTests : RepositoryTestBase
{
    private static readonly string[] ExpectedCommentTexts = { "primeiro", "segundo" };

    private CommentRepository Repo => new(Db);

    [Fact]
    public async Task AddEListagens()
    {
        var otherEvent = TestData.Event(Owner.Id, Category.Id);
        Db.Events.Add(otherEvent);
        await Db.SaveChangesAsync();

        await Repo.AddAsync(new Comment { Text = "segundo", EventId = Event.Id, UserId = Owner.Id, RegisterDate = DateTime.UtcNow });
        await Repo.AddAsync(new Comment { Text = "primeiro", EventId = Event.Id, UserId = Other.Id, RegisterDate = DateTime.UtcNow.AddMinutes(-5) });
        await Repo.AddAsync(new Comment { Text = "outro evento", EventId = otherEvent.Id, UserId = Owner.Id, RegisterDate = DateTime.UtcNow });

        var byEvent = await Repo.GetCommentsByEventAsync(Event.Id);
        Assert.Equal(ExpectedCommentTexts, byEvent.Select(c => c.Text));
        Assert.Equal("Convidado", byEvent[0].User.Name);

        Assert.Equal(3, (await Repo.GetAllAsync()).Count);
        Assert.Equal(3, (await Repo.GetByText("qualquer")).Count);
    }

    [Fact]
    public async Task MetodosNaoImplementados_Lancam()
    {
        await Assert.ThrowsAsync<NotImplementedException>(() => Repo.DeleteAsync(1));
        await Assert.ThrowsAsync<NotImplementedException>(() => Repo.UpdateAsync(new Comment()));
    }
}

public class UserRatingRepositoryTests : RepositoryTestBase
{
    private UserRatingRepository Repo => new(Db);

    [Fact]
    public async Task GetUserScoreAsync_SemAvaliacoes_RetornaZero()
    {
        Assert.Equal((0d, 0), await Repo.GetUserScoreAsync(Owner.Id));
    }

    [Fact]
    public async Task AddEConsultas()
    {
        var third = TestData.User("Terceiro");
        Db.Users.Add(third);
        await Db.SaveChangesAsync();

        await Repo.AddAsync(new UserRating { RatedUserId = Owner.Id, ReporterId = Other.Id, EventId = Event.Id, Rate = 3 });
        var second = await Repo.AddAsync(new UserRating { RatedUserId = Owner.Id, ReporterId = third.Id, EventId = Event.Id, Rate = 5 });
        await Repo.AddAsync(new UserRating { RatedUserId = Other.Id, ReporterId = Owner.Id, EventId = Event.Id, Rate = 1 });

        Assert.Equal((4d, 2), await Repo.GetUserScoreAsync(Owner.Id));
        Assert.Equal(2, (await Repo.GetRateByUser(Owner.Id)).Count);
        var all = await Repo.GetAllAsync();
        Assert.Equal(3, all.Count);
        Assert.All(all, r => Assert.NotNull(r.Event));

        second.Rate = 4;
        await Repo.UpdateRateUserAsync(second);
        Assert.Equal((3.5d, 2), await Repo.GetUserScoreAsync(Owner.Id));
    }
}

public class UserReportRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddEExists()
    {
        var repo = new UserReportRepository(Db);

        await repo.AddAsync(new UserReport
        {
            ReporterId = Owner.Id, ReportedUserId = Other.Id, EventId = Event.Id, Reason = ReportReason.Spam
        });

        Assert.True(await repo.ExistsAsync(Owner.Id, Other.Id, Event.Id, ReportReason.Spam));
        Assert.False(await repo.ExistsAsync(Owner.Id, Other.Id, Event.Id, ReportReason.Other));
    }
}
