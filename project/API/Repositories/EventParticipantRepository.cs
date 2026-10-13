using API.Data;
using API.Interfaces.Participant;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class EventParticipantRepository : IEventParticipantRepository
    {
        private readonly AppDbContext _context;

        public EventParticipantRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid eventId, Guid userId) =>
            _context.EventParticipants.AnyAsync(p => p.EventId == eventId && p.UserId == userId);

        public Task<int> CountByEventAsync(Guid eventId) =>
            _context.EventParticipants.CountAsync(p => p.EventId == eventId);

        public Task<List<User>> GetUsersByEventAsync(Guid eventId) =>
            _context.EventParticipants
                .Where(p => p.EventId == eventId)
                .OrderBy(p => p.RegisterDate)   
                .Select(p => p.User)
                .ToListAsync();

        public async Task AddAsync(EventParticipant participant)
        {
            await _context.EventParticipants.AddAsync(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(Guid eventId, Guid userId)
        {
            var removed = await _context.EventParticipants
                .Where(p => p.EventId == eventId && p.UserId == userId)
                .ExecuteDeleteAsync();

            return removed > 0;
        }
    }
}