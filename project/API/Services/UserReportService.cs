using API.Common;
using API.DTO.Report;
using API.Enums;
using API.Interfaces;
using API.Interfaces.Report;
using API.Models;
using API.Repositories;

namespace API.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        public UserReportService(IUserReportRepository userReportRepository)
        {
            _userReportRepository = userReportRepository;
        }

        public async Task<Result<UserReportDTO>> CreateReport(CreateReportDTO reportDTO)
        {
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

            var report = new UserReport
            {
                Id = reportDTO.Id,
                ReporterId = reportDTO.ReporterId,
                ReportedUserId = reportDTO.ReportedUserId,
                EventId = reportDTO.EventId,
                Reason = reportDTO.Reason,
                CustomReason = reportDTO.CustomReason,
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
                CustomReason = report.CustomReason,
                Description = report.Description,
            };
        }
    }
}