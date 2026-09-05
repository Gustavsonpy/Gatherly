namespace API.DTO.Comment
{
    public class CreateCommentDTO
    {
        public string Text { get; set; }

        public Guid EventId { get; set; }   
    }
}