using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace API.DTO.Event;

public class CreateEventDTO
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string Description { get; set; }

    [JsonRequired]
    public DateTime DateTimeStart { get; set; }
    [JsonRequired]
    public DateTime DateTimeEnd { get; set; }

    [Required(ErrorMessage = "A localização é obrigatória.")]
    [MaxLength(500)]
    public string Localization { get; set; }

    [Range(1, 1000, ErrorMessage = "A capacidade deve ser entre 1 e 1000.")]
    public int MaxCapacity { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [MaxLength(200)]
    public string City { get; set; }

    [Required(ErrorMessage = "A imagem é obrigatória.")]
    public string UrlImage { get; set; }

    [MaxLength(200)] public string? PlaceName { get; set; }
    [MaxLength(200)] public string? Street { get; set; }
    [MaxLength(100)] public string? District { get; set; }
    [MaxLength(50)]  public string? State { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude inválida.")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude inválida.")]
    public double? Longitude { get; set; }

    [JsonRequired]
    public Guid CategoryId { get; set; }
}
