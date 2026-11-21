using API.Common;
using API.DTO.Report;
using API.Enums;
using API.Interfaces;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Interfaces.Report;
using API.Models;

namespace API.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository;
        private readonly ILogger<UserReportService> _logger;

        public UserReportService(IUserReportRepository userReportRepository, IEventRepository eventRepository, IEventParticipantRepository eventParticipantRepository, ILogger<UserReportService> logger)
        {
            _userReportRepository = userReportRepository;
            _eventRepository = eventRepository;
            _eventParticipantRepository = eventParticipantRepository;
            _logger = logger;
        }

        public async Task<Result<UserReportDTO>> CreateEventReport(Guid reporterId, CreateEventReportDTO reportDTO)
        {
            var ev = await _eventRepository.GetByIdAsync(reportDTO.EventId);

            if (ev is null)
                return Result<UserReportDTO>.Failure("Evento não encontrado.");

            if (ev.UserId == reporterId)
                return Result<UserReportDTO>.Failure("Você não pode denunciar o seu próprio evento.");

            if (!Enum.IsDefined(typeof(ReportReason), reportDTO.Reason))
                return Result<UserReportDTO>.Failure("Motivo da denúncia inválido.");

            var alreadyReported = await _userReportRepository.ExistsEventReportAsync(
                reporterId,
                reportDTO.EventId,
                reportDTO.Reason
            );

            if (alreadyReported)
                return Result<UserReportDTO>.Failure("Você já denunciou este evento com o mesmo motivo.");

            var report = new UserReport
            {
                ReporterId = reporterId,
                ReportedUserId = null,
                EventId = reportDTO.EventId,
                Reason = reportDTO.Reason,
                Description = reportDTO.Description.Trim(),
            };

            var created = await _userReportRepository.AddAsync(report);

            return Result<UserReportDTO>.Success(ToReturnDTO(created));
        }

        public async Task<Result<UserReportDTO>> CreateReport(CreateReportDTO reportDTO)
        {
            var ev = await _eventRepository.GetByIdAsync(reportDTO.EventId);

            if (ev is null)
                return Result<UserReportDTO>.Failure("Evento não encontrado.");

            if (!await _eventParticipantRepository.HasJoinedAsync(ev.Id, reportDTO.ReporterId))
                return Result<UserReportDTO>.Failure("Você não participou deste evento.");

            if (!await _eventParticipantRepository.HasJoinedAsync(ev.Id, reportDTO.ReportedUserId))
                return Result<UserReportDTO>.Failure("Este usuário não participou deste evento.");

            if (reportDTO.ReporterId == reportDTO.ReportedUserId)
                return Result<UserReportDTO>.Failure("Você não pode denunciar a si mesmo.");

            if (!Enum.IsDefined(typeof(ReportReason), reportDTO.Reason))
                return Result<UserReportDTO>.Failure("Motivo da denúncia inválido.");

            var alreadyReported = await _userReportRepository.ExistsAsync(
                reportDTO.ReporterId,
                reportDTO.ReportedUserId,
                reportDTO.EventId,
                reportDTO.Reason
            );

            if (alreadyReported)
                return Result<UserReportDTO>.Failure("Você já denunciou esse mesmo usuário nesse evento com a mesma denúncia.");

            if (reportDTO.Reason == ReportReason.Other && string.IsNullOrWhiteSpace(reportDTO.Description))
                return Result<UserReportDTO>.Failure("A descrição é obrigatória");

            var report = new UserReport
            {
                ReporterId = reportDTO.ReporterId,
                ReportedUserId = reportDTO.ReportedUserId,
                EventId = reportDTO.EventId,
                Reason = reportDTO.Reason,
                Description = reportDTO.Description,
            };

            var created = await _userReportRepository.AddAsync(report);

            return Result<UserReportDTO>.Success(ToReturnDTO(created)); 
        }

        private static UserReportDTO ToReturnDTO(UserReport report)
        {
            return new()
            {
                Id = report.Id,
                ReporterId = report.ReporterId,
                ReportedUserId = report.ReportedUserId,
                EventId = report.EventId,
                Reason = report.Reason,
                Description = report.Description,
            };
        }
    }
}