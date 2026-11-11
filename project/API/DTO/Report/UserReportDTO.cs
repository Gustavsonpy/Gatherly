using API.Enums;

namespace API.DTO.Report
{
    public class UserReportDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ReporterId { get; set; }

        public Guid ReportedUserId { get; set; }

        public Guid EventId { get; set; }

        public ReportReason Reason { get; set; }

        public string? CustomReason { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}