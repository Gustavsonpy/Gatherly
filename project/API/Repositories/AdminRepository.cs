using API.Data;
using API.DTO.Admin;
using API.Interfaces.Admin;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminStatsDTO> GetStatsAsync(DateTime now) => new()
        {
            PendingReports = await _context.UserReports.CountAsync(r => r.Status == Common.ReportStatus.Pending),
            TotalUsers = await _context.Users.CountAsync(),
            SuspendedUsers = await _context.Users.CountAsync(u => u.IsSuspended),
            UpcomingEvents = await _context.Events.CountAsync(e => e.CanceledAt == null && e.DateTimeStart > now),
            TotalEvents = await _context.Events.CountAsync(),
        };

        public async Task<List<AdminReportDTO>> GetReportsAsync(string? status)
        {
            var query = _context.UserReports.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminReportDTO
                {
                    Id = r.Id,
                    ReporterId = r.ReporterId,
                    ReporterName = r.Reporter.Name,
                    ReportedUserId = r.ReportedUserId,
                    ReportedUserName = r.ReportedUser != null ? r.ReportedUser.Name : null,
                    ReportedUserSuspended = r.ReportedUser != null && r.ReportedUser.IsSuspended,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    EventCanceled = r.Event.CanceledAt != null,
                    Reason = r.Reason,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<List<AdminUserDTO>> GetUsersAsync(string? search)
        {
            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            return await query
                .OrderBy(u => u.Name)
                .Select(u => new AdminUserDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    City = u.City,
                    UrlPhoto = u.UrlPhoto,
                    IsAdmin = u.IsAdmin,
                    IsSuspended = u.IsSuspended,
                    RegisterDate = u.RegisterDate,
                    EventsCreated = u.Events.Count,
                    ReportsReceived = _context.UserReports.Count(r => r.ReportedUserId == u.Id),
                })
                .ToListAsync();
        }

        public async Task<List<AdminEventDTO>> GetEventsAsync(string? search)
        {
            var query = _context.Events.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term) || e.City.ToLower().Contains(term));
            }

            return await query
                .OrderByDescending(e => e.DateTimeStart)
                .Select(e => new AdminEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    City = e.City,
                    DateTimeStart = e.DateTimeStart,
                    DateTimeEnd = e.DateTimeEnd,
                    CanceledAt = e.CanceledAt,
                    OrganizerId = e.UserId,
                    OrganizerName = e.User.Name,
                    CategoryName = e.Category.Name,
                    ParticipantsCount = e.Participants.Count(p => p.LeftAt == null),
                    MaxCapacity = e.MaxCapacity,
                    ReportsCount = _context.UserReports.Count(r => r.EventId == e.Id && r.ReportedUserId == null),
                })
                .ToListAsync();
        }

        public async Task<List<AdminCommentDTO>> GetCommentsAsync(string? search, int limit)
        {
            var query = _context.Comments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Text.ToLower().Contains(term) || c.User.Name.ToLower().Contains(term));
            }

            return await query
                .OrderByDescending(c => c.RegisterDate)
                .Take(limit)
                .Select(c => new AdminCommentDTO
                {
                    Id = c.Id,
                    Text = c.Text,
                    RegisterDate = c.RegisterDate,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    EventId = c.EventId,
                    EventTitle = c.Event.Title,
                })
                .ToListAsync();
        }

        public Task<UserReport?> GetReportAsync(Guid id) =>
            _context.UserReports.FirstOrDefaultAsync(r => r.Id == id);

        public Task<User?> GetUserAsync(Guid id) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<Event?> GetEventAsync(Guid id) =>
            _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        public Task<Comment?> GetCommentAsync(Guid id) =>
            _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

        public void RemoveComment(Comment comment) => _context.Comments.Remove(comment);

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
