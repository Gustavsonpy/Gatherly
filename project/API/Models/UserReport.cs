
using API.Enums;

namespace API.Models
{
    public class UserReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = null!;

        public Guid ReportedUserId { get; set; }
        public User ReportedUser { get; set; } = null!;

        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;

        public ReportReason Reason { get; set; }

        public string? CustomReason { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
