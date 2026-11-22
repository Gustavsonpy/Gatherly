using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace API.DTO.Comment
{
    public class CreateCommentDTO
    {
        [Required(ErrorMessage = "O comentário não pode ser vazio.")]
        [MaxLength(500, ErrorMessage = "O comentário deve ter no máximo 500 caracteres.")]
        public string Text { get; set; }

        [JsonRequired]
        public Guid EventId { get; set; }
    }
}