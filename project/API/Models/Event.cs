using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Localization { get; set; }
        public int MaxCapacity { get; set; }
        public string City { get; set; }
        public string UrlImage { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? PlaceName { get; set; }
        public string? Street { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }


        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
    }
}