namespace API.DTO.Event
{
    public class UpdateEventDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Localization { get; set; }
        public int MaxCapacity { get; set; }
        public string City { get; set; }
        public string? Level { get; set; }
        public string UrlImage { get; set; }
        public string? PlaceName { get; set; }
        public string? Street { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid CategoryId { get; set; }
    }
}
