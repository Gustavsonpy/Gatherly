using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventModel = API.Models.Event;

namespace API.Interfaces.Event
{
    public interface IEventRepository
    {
        Task<EventModel?> GetByIdAsync(int id);
        Task<EventModel?> GetByTitle(string title);
        Task<List<EventModel>> GetAllAsync();
        Task<EventModel> AddAsync(EventModel informal_event);
        Task UpdateAsync(EventModel informal_event);
        Task DeleteAsync(int id);
    }
}