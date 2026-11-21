using API.Common;
using API.DTO.Admin;

namespace API.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<AdminStatsDTO> GetStatsAsync();
        Task<Result<List<AdminReportDTO>>> GetReportsAsync(string? status);
        Task<List<AdminUserDTO>> GetUsersAsync(string? search);
        Task<List<AdminEventDTO>> GetEventsAsync(string? search);
        Task<List<AdminCommentDTO>> GetCommentsAsync(string? search);

        Task<Result<bool>> UpdateReportStatusAsync(Guid adminId, Guid reportId, string status);
        Task<Result<bool>> SetUserSuspendedAsync(Guid adminId, Guid userId, bool suspended);
        Task<Result<bool>> SetUserAdminAsync(Guid adminId, Guid userId, bool isAdmin);
        Task<Result<bool>> CancelEventAsync(Guid adminId, Guid eventId);
        Task<Result<bool>> DeleteCommentAsync(Guid adminId, Guid commentId);
    }
}
