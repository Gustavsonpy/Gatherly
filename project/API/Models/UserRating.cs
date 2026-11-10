namespace API.Models
{
    public class UserRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Rate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public Guid ReporterId { get; set; }
        public User? Reporter { get; set; }

        public Guid RatedUserId { get; set; }
        public User? RatedUser { get; set; }

        public Guid EventId { get; set; }
        public Event? Event { get; set; }
        
    }
}