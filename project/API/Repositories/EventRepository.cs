using API.Data;
using API.DTO.Event;
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

        public async Task DeleteAsync(EventModel informal_event)
        {
            _context.Events.Remove(informal_event);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasReportsOrRatingsAsync(Guid eventId)
        {
            return await _context.UserReports.AnyAsync(r => r.EventId == eventId)
                || await _context.UserRatings.AnyAsync(r => r.EventId == eventId);
        }

        public async Task<List<EventModel>> GetAllAsync()
        {
            return await _context.Events
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .Include(e => e.Participants)
                    .OrderBy(e => e.Title)
                    .ToListAsync();
        }

        public async Task<EventModel?> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<EventModel?> GetByTitle(string title)
        {
            return await _context.Events.FirstOrDefaultAsync(x => x.Title == title);
        }

        public async Task<List<EventModel>> GetByCityAsync(string city)
        {
            var now = DateTime.UtcNow;
            
            return await _context.Events
                .Where(e => e.City == city && e.DateTimeStart > now && e.CanceledAt == null)
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .OrderBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<EventModel>> GetByUserAsync(Guid userId)
        {
            return await _context.Events
                .Where(e => e.UserId == userId
                        || e.Participants.Any(p => p.UserId == userId))
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .OrderBy(e => e.DateTimeStart)
                .ToListAsync();
        }

        public async Task UpdateAsync(EventModel informal_event)
        {
            _context.Events.Update(informal_event);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EventModel>> GetFutureEvents()
        {
            var now = DateTime.UtcNow;

            return await _context.Events
                .Where(e => e.DateTimeStart > now && e.CanceledAt == null)
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .OrderBy(e => e.DateTimeStart)
                .ToListAsync();
                }
    }
}