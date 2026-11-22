using System.Text.Json.Serialization;
using API.Enums;

namespace API.DTO.Report
{
    public class CreateReportDTO
    {
        [JsonRequired]
        public Guid ReportedUserId { get; set; }
        [JsonRequired]
        public Guid EventId { get; set; }
        [JsonRequired]
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}