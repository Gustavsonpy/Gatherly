using System;
 
namespace API.DTO
{
    public class UpdateUserDTO
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
    }
}