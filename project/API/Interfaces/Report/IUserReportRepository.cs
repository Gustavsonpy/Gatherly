using API.Enums;
using API.Models;

namespace API.Interfaces
{
    public interface IUserReportRepository
    {
        Task<UserReport> AddAsync(UserReport userReport);
        Task<bool> ExistsAsync(Guid reporterId, Guid reportedUserId, Guid eventId, ReportReason reason);
    }
}