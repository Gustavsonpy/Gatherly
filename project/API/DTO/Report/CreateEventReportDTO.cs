using API.Enums;

namespace API.DTO.Report
{
    public class CreateEventReportDTO
    {
        public Guid EventId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
