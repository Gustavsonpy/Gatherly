using EventModel = API.Models.Event;
using UserModel = API.Models.User;

namespace API.DTO.Comment
{
    public class CommentDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; }
        public DateTime RegisterDate { get; set; }

        public Guid EventId { get; set; }

        public Guid UserId { get; set; }
        public string CommentUserName { get; set; }
        public string? UserPhoto { get; set; }
    }
}