namespace API.DTO.Event
{
    public class ReturnEventDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Localization { get; set; }
        public int MaxCapacity { get; set; }
        public string City { get; set; }
        public string Level { get; set; }
        public string UrlImage { get; set; }
        public DateTime RegisterDate { get; set; }
        public int ParticipantsCount { get; set; }
        public bool IsParticipating { get; set; }

        public Guid UserId { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorPhoto { get; set; }

        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}