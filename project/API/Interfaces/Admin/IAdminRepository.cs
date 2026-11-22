using API.DTO.Admin;
using API.Models;
using EventModel = API.Models.Event;
using CommentModel = API.Models.Comment;

namespace API.Interfaces.Admin
{
    public interface IAdminRepository
    {
        Task<AdminStatsDTO> GetStatsAsync(DateTime now);
        Task<List<AdminReportDTO>> GetReportsAsync(string? status);
        Task<List<AdminUserDTO>> GetUsersAsync(string? search);
        Task<List<AdminEventDTO>> GetEventsAsync(string? search);
        Task<List<AdminCommentDTO>> GetCommentsAsync(string? search, int limit);

        Task<UserReport?> GetReportAsync(Guid id);
        Task<User?> GetUserAsync(Guid id);
        Task<EventModel?> GetEventAsync(Guid id);
        Task<CommentModel?> GetCommentAsync(Guid id);

        void RemoveComment(CommentModel comment);
        Task SaveChangesAsync();
    }
}
