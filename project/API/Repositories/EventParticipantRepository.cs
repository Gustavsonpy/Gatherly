using API.Data;
using API.Interfaces.Participant;
using API.Models;
using Microsoft.EntityFrameworkCore;
using static API.Interfaces.Participant.IEventParticipantRepository;

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

        public async Task<JoinEventStatus> JoinAsync(Guid eventId, Guid userId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var ev = await _context.Events
                .FromSqlInterpolated($@"SELECT * FROM ""Events"" WHERE ""Id"" = {eventId} FOR UPDATE")
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (ev is null)
                return JoinEventStatus.EventNotFound;

            if (await _context.EventParticipants.AnyAsync(p => p.EventId == eventId && p.UserId == userId))
                return JoinEventStatus.AlreadyJoined;

            var count = await _context.EventParticipants.CountAsync(p => p.EventId == eventId);

            if (count >= ev.MaxCapacity)
                return JoinEventStatus.Full;

            _context.EventParticipants.Add(new EventParticipant
            {
                EventId = eventId,
                UserId = userId,
                RegisterDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return JoinEventStatus.Joined;
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