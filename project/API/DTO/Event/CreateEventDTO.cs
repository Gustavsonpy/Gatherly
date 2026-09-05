namespace API.DTO.Event
{
    public class CreateEventDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Localization { get; set; }
        public int MaxCapacity { get; set; }
        public string City { get; set; }
        public string? Level { get; set; }
        public string UrlImage { get; set; }
        public Guid CategoryId { get; set; }
    }
}