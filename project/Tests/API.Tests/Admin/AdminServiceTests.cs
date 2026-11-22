using API.Common;
using API.Data;
using API.Enums;
using API.Models;
using API.Repositories;
using API.Services;
using API.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests.Admin;

public class AdminServiceTests
{
    private readonly AppDbContext _db = TestDb.Create();
    private readonly AdminService _service;

    private readonly User _admin;
    private readonly User _owner;
    private readonly User _other;
    private readonly Event _event;

    public AdminServiceTests()
    {
        _service = new AdminService(new AdminRepository(_db), new ReportReasonService(), NullLogger<AdminService>.Instance);

        _admin = TestData.User("Admin Chefe", "admin@email.com");
        _admin.IsAdmin = true;
        _owner = TestData.User("Ana Organizadora", "ana@email.com");
        _other = TestData.User("Bruno Participante", "bruno@email.com");
        var category = TestData.Category();
        _db.AddRange(_admin, _owner, _other, category);
        _db.SaveChanges();

        _event = TestData.Event(_owner.Id, category.Id, title: "Futebol de domingo");
        _db.Events.Add(_event);
        _db.SaveChanges();
    }

    private UserReport AddReport(Guid? reportedUserId, ReportReason reason = ReportReason.Spam, string status = ReportStatus.Pending)
    {
        var report = new UserReport
        {
            ReporterId = _other.Id,
            ReportedUserId = reportedUserId,
            EventId = _event.Id,
            Reason = reason,
            Description = "descrição",
            Status = status,
        };
        _db.UserReports.Add(report);
        _db.SaveChanges();
        return report;
    }

    // ---------- Stats ----------

    [Fact]
    public async Task GetStats_ContaRegistros()
    {
        AddReport(_owner.Id);
        AddReport(null, status: ReportStatus.Resolved);
        _other.IsSuspended = true;
        _db.SaveChanges();

        var stats = await _service.GetStatsAsync();

        Assert.Equal(1, stats.PendingReports);
        Assert.Equal(3, stats.TotalUsers);
        Assert.Equal(1, stats.SuspendedUsers);
        Assert.Equal(1, stats.UpcomingEvents);
        Assert.Equal(1, stats.TotalEvents);
    }

    // ---------- Denúncias ----------

    [Fact]
    public async Task GetReports_TrazNomesETraduzMotivo()
    {
        AddReport(_owner.Id, ReportReason.InappropriateBehavior);

        var result = await _service.GetReportsAsync(null);

        var report = Assert.Single(result.Value!);
        Assert.Equal("Bruno Participante", report.ReporterName);
        Assert.Equal("Ana Organizadora", report.ReportedUserName);
        Assert.Equal("Futebol de domingo", report.EventTitle);
        Assert.Equal("Comportamento inadequado", report.ReasonName);
    }

    [Fact]
    public async Task GetReports_FiltraPorStatus()
    {
        AddReport(_owner.Id);
        AddReport(null, status: ReportStatus.Dismissed);

        var result = await _service.GetReportsAsync(ReportStatus.Dismissed);

        var report = Assert.Single(result.Value!);
        Assert.Null(report.ReportedUserId);
    }

    [Fact]
    public async Task GetReports_StatusInvalido_Falha()
    {
        var result = await _service.GetReportsAsync("Qualquer");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateReportStatus_AlteraStatus()
    {
        var report = AddReport(_owner.Id);

        var result = await _service.UpdateReportStatusAsync(_admin.Id, report.Id, ReportStatus.Resolved);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReportStatus.Resolved, _db.UserReports.Single().Status);
    }

    [Fact]
    public async Task UpdateReportStatus_StatusInvalido_Falha()
    {
        var report = AddReport(_owner.Id);
        var result = await _service.UpdateReportStatusAsync(_admin.Id, report.Id, "Aprovado");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateReportStatus_Inexistente_Falha()
    {
        var result = await _service.UpdateReportStatusAsync(_admin.Id, Guid.NewGuid(), ReportStatus.Resolved);
        Assert.Equal("Denúncia não encontrada.", Assert.Single(result.Errors));
    }

    // ---------- Usuários ----------

    [Fact]
    public async Task GetUsers_BuscaPorNomeOuEmailEContaDenuncias()
    {
        AddReport(_owner.Id);

        var porNome = await _service.GetUsersAsync("ana");
        var porEmail = await _service.GetUsersAsync("BRUNO@");

        var ana = Assert.Single(porNome);
        Assert.Equal(1, ana.ReportsReceived);
        Assert.Equal(1, ana.EventsCreated);
        Assert.Equal(_other.Id, Assert.Single(porEmail).Id);
    }

    [Fact]
    public async Task GetUsers_SemBusca_RetornaTodosOrdenados()
    {
        var users = await _service.GetUsersAsync(null);
        Assert.Equal(["Admin Chefe", "Ana Organizadora", "Bruno Participante"], users.Select(u => u.Name));
    }

    [Fact]
    public async Task SetUserSuspended_SuspendeEReativa()
    {
        Assert.True((await _service.SetUserSuspendedAsync(_admin.Id, _other.Id, true)).IsSuccess);
        Assert.True(_db.Users.Single(u => u.Id == _other.Id).IsSuspended);

        Assert.True((await _service.SetUserSuspendedAsync(_admin.Id, _other.Id, false)).IsSuccess);
        Assert.False(_db.Users.Single(u => u.Id == _other.Id).IsSuspended);
    }

    [Fact]
    public async Task SetUserSuspended_ASiMesmo_Falha()
    {
        var result = await _service.SetUserSuspendedAsync(_admin.Id, _admin.Id, true);
        Assert.Equal("Você não pode suspender a si mesmo.", Assert.Single(result.Errors));
    }

    [Fact]
    public async Task SetUserSuspended_OutroAdmin_Falha()
    {
        _other.IsAdmin = true;
        _db.SaveChanges();

        var result = await _service.SetUserSuspendedAsync(_admin.Id, _other.Id, true);

        Assert.False(result.IsSuccess);
        Assert.False(_other.IsSuspended);
    }

    [Fact]
    public async Task SetUserSuspended_Inexistente_Falha()
    {
        var result = await _service.SetUserSuspendedAsync(_admin.Id, Guid.NewGuid(), true);
        Assert.Equal("Usuário não encontrado.", Assert.Single(result.Errors));
    }

    [Fact]
    public async Task SetUserAdmin_ConcedeERemove()
    {
        Assert.True((await _service.SetUserAdminAsync(_admin.Id, _other.Id, true)).IsSuccess);
        Assert.True(_other.IsAdmin);

        Assert.True((await _service.SetUserAdminAsync(_admin.Id, _other.Id, false)).IsSuccess);
        Assert.False(_other.IsAdmin);
    }

    [Fact]
    public async Task SetUserAdmin_RemoverOProprio_Falha()
    {
        var result = await _service.SetUserAdminAsync(_admin.Id, _admin.Id, false);
        Assert.False(result.IsSuccess);
        Assert.True(_admin.IsAdmin);
    }

    [Fact]
    public async Task SetUserAdmin_UsuarioSuspenso_Falha()
    {
        _other.IsSuspended = true;
        _db.SaveChanges();

        var result = await _service.SetUserAdminAsync(_admin.Id, _other.Id, true);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SetUserAdmin_Inexistente_Falha()
    {
        var result = await _service.SetUserAdminAsync(_admin.Id, Guid.NewGuid(), true);
        Assert.Equal("Usuário não encontrado.", Assert.Single(result.Errors));
    }

    // ---------- Eventos ----------

    [Fact]
    public async Task GetEvents_CalculaStatusEContagens()
    {
        var category = _db.Categories.Single();
        _db.Events.AddRange(
            TestData.Event(_owner.Id, category.Id, start: DateTime.UtcNow.AddHours(-1), end: DateTime.UtcNow.AddHours(1), title: "Em andamento"),
            TestData.Event(_owner.Id, category.Id, start: DateTime.UtcNow.AddDays(-2), end: DateTime.UtcNow.AddDays(-2).AddHours(2), title: "Terminado"),
            TestData.Event(_owner.Id, category.Id, title: "Cancelado", canceledAt: DateTime.UtcNow));
        _db.EventParticipants.AddRange(
            TestData.Participant(_event.Id, _owner.Id),
            TestData.Participant(_event.Id, _other.Id, leftAt: DateTime.UtcNow));
        _db.SaveChanges();
        AddReport(null);

        var events = (await _service.GetEventsAsync(null)).ToDictionary(e => e.Title);

        Assert.Equal("upcoming", events["Futebol de domingo"].Status);
        Assert.Equal(1, events["Futebol de domingo"].ParticipantsCount);
        Assert.Equal(1, events["Futebol de domingo"].ReportsCount);
        Assert.Equal("Ana Organizadora", events["Futebol de domingo"].OrganizerName);
        Assert.Equal("ongoing", events["Em andamento"].Status);
        Assert.Equal("finished", events["Terminado"].Status);
        Assert.Equal("canceled", events["Cancelado"].Status);
    }

    [Fact]
    public async Task GetEvents_BuscaPorTituloOuCidade()
    {
        Assert.Single(await _service.GetEventsAsync("futebol"));
        Assert.Single(await _service.GetEventsAsync("curitiba"));
        Assert.Empty(await _service.GetEventsAsync("xadrez"));
    }

    [Fact]
    public async Task CancelEvent_Cancela()
    {
        var result = await _service.CancelEventAsync(_admin.Id, _event.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(_db.Events.Single().CanceledAt);
    }

    [Fact]
    public async Task CancelEvent_JaCancelado_Falha()
    {
        _event.CanceledAt = DateTime.UtcNow;
        _db.SaveChanges();

        var result = await _service.CancelEventAsync(_admin.Id, _event.Id);

        Assert.Equal("O evento já está cancelado.", Assert.Single(result.Errors));
    }

    [Fact]
    public async Task CancelEvent_JaTerminou_Falha()
    {
        _event.DateTimeStart = DateTime.UtcNow.AddDays(-1);
        _event.DateTimeEnd = DateTime.UtcNow.AddHours(-20);
        _db.SaveChanges();

        var result = await _service.CancelEventAsync(_admin.Id, _event.Id);

        Assert.False(result.IsSuccess);
        Assert.Null(_event.CanceledAt);
    }

    [Fact]
    public async Task CancelEvent_Inexistente_Falha()
    {
        var result = await _service.CancelEventAsync(_admin.Id, Guid.NewGuid());
        Assert.Equal("Evento não encontrado.", Assert.Single(result.Errors));
    }

    // ---------- Comentários ----------

    private Comment AddComment(string text, Guid userId, DateTime? date = null)
    {
        var comment = new Comment { Text = text, UserId = userId, EventId = _event.Id, RegisterDate = date ?? DateTime.UtcNow };
        _db.Comments.Add(comment);
        _db.SaveChanges();
        return comment;
    }

    [Fact]
    public async Task GetComments_MaisRecentesPrimeiroEComBusca()
    {
        AddComment("Primeiro", _owner.Id, DateTime.UtcNow.AddMinutes(-10));
        AddComment("Conteúdo ofensivo", _other.Id);

        var all = await _service.GetCommentsAsync(null);
        var filtered = await _service.GetCommentsAsync("ofensivo");
        var byUser = await _service.GetCommentsAsync("ana");

        Assert.Equal(["Conteúdo ofensivo", "Primeiro"], all.Select(c => c.Text));
        Assert.Equal("Futebol de domingo", all[0].EventTitle);
        Assert.Equal("Bruno Participante", Assert.Single(filtered).UserName);
        Assert.Equal("Primeiro", Assert.Single(byUser).Text);
    }

    [Fact]
    public async Task DeleteComment_Remove()
    {
        var comment = AddComment("Spam", _other.Id);

        var result = await _service.DeleteCommentAsync(_admin.Id, comment.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(_db.Comments);
    }

    [Fact]
    public async Task DeleteComment_Inexistente_Falha()
    {
        var result = await _service.DeleteCommentAsync(_admin.Id, Guid.NewGuid());
        Assert.Equal("Comentário não encontrado.", Assert.Single(result.Errors));
    }
}
