namespace API.DTO.Rate
{
    public class CreateUserRatingDTO
    {
        public int Rate { get; set; }
        public string? Description { get; set; }

        public Guid ReporterId { get; set; }
        public Guid RatedUserId { get; set; }

        public Guid EventId { get; set; }
    }
}