using API.Common;
using API.DTO.Report;
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

            var crated = await _userReportRepository.AddAsync(report);

            var reportDto = ToReturnDTO(crated);

            return Result<UserReportDTO>.Success(reportDto); 
            
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