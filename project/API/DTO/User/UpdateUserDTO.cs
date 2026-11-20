using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class UpdateUserDTO
    {
        [MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        public DateTime Birthday { get; set; }

        [MaxLength(500, ErrorMessage = "A biografia deve ter no máximo 500 caracteres.")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string? City { get; set; }

    }
}