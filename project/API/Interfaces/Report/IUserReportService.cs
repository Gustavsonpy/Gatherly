using API.Common;
using API.DTO.Report;

namespace API.Interfaces.Report
{
    public interface IUserReportService
    {
        Task<Result<UserReportDTO>> CreateReport(CreateReportDTO reportDTO);
    }
}