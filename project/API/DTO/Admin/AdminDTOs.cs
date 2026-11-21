using API.Enums;

namespace API.DTO.Admin
{
    public class AdminStatsDTO
    {
        public int PendingReports { get; set; }
        public int TotalUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalEvents { get; set; }
    }

    public class AdminReportDTO
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public Guid? ReportedUserId { get; set; }
        public string? ReportedUserName { get; set; }
        public bool ReportedUserSuspended { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public bool EventCanceled { get; set; }
        public ReportReason Reason { get; set; }
        public string ReasonName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? UrlPhoto { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime RegisterDate { get; set; }
        public int EventsCreated { get; set; }
        public int ReportsReceived { get; set; }
    }

    public class AdminEventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ParticipantsCount { get; set; }
        public int MaxCapacity { get; set; }
        public int ReportsCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AdminCommentDTO
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
    }

    public class UpdateReportStatusDTO
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateFlagDTO
    {
        public bool Value { get; set; }
    }
}
