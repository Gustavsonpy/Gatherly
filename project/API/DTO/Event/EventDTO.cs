using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserModel = API.Models.User;
using CategoryModel = API.Models.Category;

namespace API.DTO.Event
{
    public class EventDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Localization { get; set; }
        public int MaxCapacity { get; set; }
        public string City { get; set; }
        public string Level { get; set; }
        public string UrlImage { get; set; }
        public DateTime RegisterDate { get; set; }

        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public Guid CategoryId { get; set; }
        public CategoryModel Category { get; set; }
    }
}