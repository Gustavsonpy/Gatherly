using API.DTO.Report;
using API.Enums;
using API.Interfaces;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Models;
using API.Services;
using API.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Services;

public class UserReportServiceTests
{
    private readonly Mock<IUserReportRepository> _reports = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventParticipantRepository> _participants = new();
    private readonly Guid _reporter = Guid.NewGuid();
    private readonly Guid _reported = Guid.NewGuid();
    private readonly Event _event = TestData.Event(Guid.NewGuid(), Guid.NewGuid());

    private UserReportService CreateService() =>
        new(_reports.Object, _events.Object, _participants.Object, NullLogger<UserReportService>.Instance);

    private CreateReportDTO Dto(ReportReason reason = ReportReason.Spam, string description = "") => new()
    {
        ReporterId = _reporter, ReportedUserId = _reported, EventId = _event.Id, Reason = reason, Description = description
    };

    private void SetupJoined(bool reporter = true, bool reported = true)
    {
        _events.Setup(r => r.GetByIdAsync(_event.Id)).ReturnsAsync(_event);
        _participants.Setup(r => r.HasJoinedAsync(_event.Id, _reporter)).ReturnsAsync(reporter);
        _participants.Setup(r => r.HasJoinedAsync(_event.Id, _reported)).ReturnsAsync(reported);
    }

    [Fact]
    public async Task CreateReport_EventoInexistente_RetornaErro()
    {
        var result = await CreateService().CreateReport(Dto());
        Assert.Equal("Evento não encontrado.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_DenuncianteNaoParticipou_RetornaErro()
    {
        SetupJoined(reporter: false);
        var result = await CreateService().CreateReport(Dto());
        Assert.Equal("Você não participou deste evento.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_DenunciadoNaoParticipou_RetornaErro()
    {
        SetupJoined(reported: false);
        var result = await CreateService().CreateReport(Dto());
        Assert.Equal("Este usuário não participou deste evento.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_AutoDenuncia_RetornaErro()
    {
        _events.Setup(r => r.GetByIdAsync(_event.Id)).ReturnsAsync(_event);
        _participants.Setup(r => r.HasJoinedAsync(_event.Id, _reporter)).ReturnsAsync(true);
        var dto = Dto();
        dto.ReportedUserId = _reporter;

        var result = await CreateService().CreateReport(dto);

        Assert.Equal("Você não pode denunciar a si mesmo.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_MotivoInvalido_RetornaErro()
    {
        SetupJoined();
        var result = await CreateService().CreateReport(Dto((ReportReason)99));
        Assert.Equal("Motivo da denúncia inválido.", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_Duplicada_RetornaErro()
    {
        SetupJoined();
        _reports.Setup(r => r.ExistsAsync(_reporter, _reported, _event.Id, ReportReason.Spam)).ReturnsAsync(true);

        var result = await CreateService().CreateReport(Dto());

        Assert.Contains("Você já denunciou", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_MotivoOutroSemDescricao_RetornaErro()
    {
        SetupJoined();
        var result = await CreateService().CreateReport(Dto(ReportReason.Other, "  "));
        Assert.Equal("A descrição é obrigatória", result.Errors.Single());
    }

    [Fact]
    public async Task CreateReport_Valida_SalvaDenuncia()
    {
        SetupJoined();
        _reports.Setup(r => r.AddAsync(It.IsAny<UserReport>())).ReturnsAsync((UserReport r) => r);

        var result = await CreateService().CreateReport(Dto(ReportReason.Other, "Comportamento agressivo"));

        Assert.True(result.IsSuccess);
        Assert.Equal(ReportReason.Other, result.Value!.Reason);
        Assert.Equal("Comportamento agressivo", result.Value.Description);
        Assert.Equal(_reported, result.Value.ReportedUserId);
    }
}
