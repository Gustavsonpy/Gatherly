using API.Common;
using API.DTO.Admin;
using API.Interfaces.Admin;

namespace API.Services
{
    public class AdminService : IAdminService
    {
        private const int CommentsLimit = 100;

        private readonly IAdminRepository _repository;
        private readonly ReportReasonService _reasonService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(IAdminRepository repository, ReportReasonService reasonService, ILogger<AdminService> logger)
        {
            _repository = repository;
            _reasonService = reasonService;
            _logger = logger;
        }

        public Task<AdminStatsDTO> GetStatsAsync() => _repository.GetStatsAsync(DateTime.UtcNow);

        public async Task<Result<List<AdminReportDTO>>> GetReportsAsync(string? status)
        {
            if (!string.IsNullOrWhiteSpace(status) && !ReportStatus.IsValid(status))
                return Result<List<AdminReportDTO>>.Failure("Status de denúncia inválido.");

            var reports = await _repository.GetReportsAsync(status);
            var names = _reasonService.GetAll().ToDictionary(r => r.Id, r => r.Name);

            foreach (var report in reports)
                report.ReasonName = names.GetValueOrDefault((int)report.Reason, report.Reason.ToString());

            return Result<List<AdminReportDTO>>.Success(reports);
        }

        public Task<List<AdminUserDTO>> GetUsersAsync(string? search) => _repository.GetUsersAsync(search);

        public async Task<List<AdminEventDTO>> GetEventsAsync(string? search)
        {
            var events = await _repository.GetEventsAsync(search);
            var now = DateTime.UtcNow;

            foreach (var ev in events)
                ev.Status = ev.CanceledAt is not null ? "canceled"
                    : now < ev.DateTimeStart ? "upcoming"
                    : now < ev.DateTimeEnd ? "ongoing"
                    : "finished";

            return events;
        }

        public Task<List<AdminCommentDTO>> GetCommentsAsync(string? search) =>
            _repository.GetCommentsAsync(search, CommentsLimit);

        public async Task<Result<bool>> UpdateReportStatusAsync(Guid adminId, Guid reportId, string status)
        {
            if (!ReportStatus.IsValid(status))
                return Result<bool>.Failure("Status de denúncia inválido.");

            var report = await _repository.GetReportAsync(reportId);
            if (report is null)
                return Result<bool>.Failure("Denúncia não encontrada.");

            report.Status = status;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} alterou a denúncia {ReportId} para {Status}", adminId, reportId, status);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> SetUserSuspendedAsync(Guid adminId, Guid userId, bool suspended)
        {
            if (adminId == userId)
                return Result<bool>.Failure("Você não pode suspender a si mesmo.");

            var user = await _repository.GetUserAsync(userId);
            if (user is null)
                return Result<bool>.Failure("Usuário não encontrado.");

            if (suspended && user.IsAdmin)
                return Result<bool>.Failure("Remova o acesso de administrador antes de suspender este usuário.");

            user.IsSuspended = suspended;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} {Action} o usuário {UserId}",
                adminId, suspended ? "suspendeu" : "reativou", userId);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> SetUserAdminAsync(Guid adminId, Guid userId, bool isAdmin)
        {
            if (adminId == userId && !isAdmin)
                return Result<bool>.Failure("Você não pode remover o seu próprio acesso de administrador.");

            var user = await _repository.GetUserAsync(userId);
            if (user is null)
                return Result<bool>.Failure("Usuário não encontrado.");

            if (isAdmin && user.IsSuspended)
                return Result<bool>.Failure("Um usuário suspenso não pode ser administrador.");

            user.IsAdmin = isAdmin;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} {Action} acesso de administrador do usuário {UserId}",
                adminId, isAdmin ? "concedeu" : "removeu", userId);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelEventAsync(Guid adminId, Guid eventId)
        {
            var ev = await _repository.GetEventAsync(eventId);
            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.CanceledAt is not null)
                return Result<bool>.Failure("O evento já está cancelado.");

            if (ev.DateTimeEnd <= DateTime.UtcNow)
                return Result<bool>.Failure("Não é possível cancelar um evento que já terminou.");

            ev.CanceledAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} cancelou o evento {EventId}", adminId, eventId);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteCommentAsync(Guid adminId, Guid commentId)
        {
            var comment = await _repository.GetCommentAsync(commentId);
            if (comment is null)
                return Result<bool>.Failure("Comentário não encontrado.");

            _repository.RemoveComment(comment);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} removeu o comentário {CommentId} do evento {EventId}",
                adminId, commentId, comment.EventId);
            return Result<bool>.Success(true);
        }
    }
}
