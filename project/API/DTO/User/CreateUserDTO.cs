using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe uma senha.")]
        [MinLength(6, ErrorMessage = "A senha precisa ter 6 caracteres ou mais.")]
        [MaxLength(72, ErrorMessage = "A senha deve ter no máximo 72 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [JsonRequired]
        public DateTime Birthday { get; set; }
    }
}