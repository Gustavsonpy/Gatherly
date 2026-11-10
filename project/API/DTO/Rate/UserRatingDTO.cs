namespace API.DTO.Rate
{
    public class UserRatingDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Rate { get; set; }
        public string? Description { get; set; }
        public Guid ReporterId { get; set; }
        public Guid RatedUserId { get; set; }

        public Guid EventId { get; set; }
    }
}