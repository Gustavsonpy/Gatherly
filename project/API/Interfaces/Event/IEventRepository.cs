using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventModel = API.Models.Event;

namespace API.Interfaces.Event
{
    public interface IEventRepository
    {
        Task<EventModel?> GetByIdAsync(Guid id);
        Task<EventModel?> GetByTitle(string title);
        Task<List<EventModel>> GetByCityAsync(string city, string? state);
        Task<List<EventModel>> GetAllAsync();
        Task<List<EventModel>> GetByUserAsync(Guid userId);
        Task<EventModel> AddAsync(EventModel informal_event);
        Task UpdateAsync(EventModel informal_event);
        Task<List<EventModel>> GetFutureEvents();
        Task DeleteAsync(EventModel informal_event);
        Task<bool> HasReportsOrRatingsAsync(Guid eventId);
    }
}