namespace API.Models
{
    public class EventParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime RegisterDate { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid EventId { get; set; }
        public Event Event { get; set; }
    }
}