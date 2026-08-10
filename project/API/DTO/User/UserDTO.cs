using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Birthday { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? UrlPhoto { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}