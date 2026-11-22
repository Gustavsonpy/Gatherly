using System.Text.Json.Serialization;

namespace API.DTO.Rate
{
    public class CreateUserRatingDTO
    {
        [JsonRequired]
        public int Rate { get; set; }
        public string? Description { get; set; }

        [JsonRequired]
        public Guid RatedUserId { get; set; }

        [JsonRequired]
        public Guid EventId { get; set; }
    }
}