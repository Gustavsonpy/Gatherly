using API.Data;
using API.Interfaces.Event;
using Microsoft.EntityFrameworkCore;
using EventModel = API.Models.Event;

namespace API.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EventModel> AddAsync(EventModel informal_event)
        {
            await _context.Events.AddAsync(informal_event);

            await _context.SaveChangesAsync();

            return informal_event;
            
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<EventModel>> GetAllAsync()
        {
            return await _context.Events
                    .OrderBy(e => e.Title)
                    .ToListAsync();
        }

        public Task<EventModel?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<EventModel?> GetByTitle(string title)
        {
            return await _context.Events.FirstOrDefaultAsync(x => x.Title == title);
        }

        public Task UpdateAsync(EventModel informal_event)
        {
            throw new NotImplementedException();
        }
    }
}