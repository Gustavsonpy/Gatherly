using API.Data;
using API.DTO.Report;
using API.Enums;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> ExistsAsync(Guid reporterId, Guid reportedUserId, Guid eventId, ReportReason reason)
        {
            return await _context.UserReports.AnyAsync(r =>
                r.ReporterId == reporterId &&
                r.ReportedUserId == reportedUserId &&
                r.EventId == eventId &&
                r.Reason == reason);
        }
    }
}