using API.Models;

namespace API.Interfaces
{
    public interface IUserReportRepository
    {
        Task<UserReport> AddAsync(UserReport userReport);
    }
}