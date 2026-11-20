using API.Enums;

namespace API.DTO.Report
{
    public class CreateReportDTO
    {
        public Guid ReporterId { get; set; }
        public Guid ReportedUserId { get; set; }
        public Guid EventId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}