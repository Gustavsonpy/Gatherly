namespace API.DTO.Event
{
    public class ParticipantDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? UrlPhoto { get; set; }
        public double UserScore { get; set; }
        public int RatingsCount { get; set; }
    }
}