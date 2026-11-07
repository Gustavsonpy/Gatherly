using API.Data;
using API.DTO.Report;
using API.Interfaces;
using API.Models;

namespace API.Repositories
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly AppDbContext _context;

        public UserReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserReport> AddAsync(UserReport userReport)
        {
            await _context.UserReports.AddAsync(userReport);
            await _context.SaveChangesAsync();
            
            return userReport;
        }
    }
}