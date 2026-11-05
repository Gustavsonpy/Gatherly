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
            _context.EventParticipants.AnyAsync(p =>
                p.EventId == eventId &&
                p.UserId == userId &&
                p.LeftAt == null);

        public Task<int> CountByEventAsync(Guid eventId) =>
            _context.EventParticipants.CountAsync(p =>
                p.EventId == eventId &&
                p.LeftAt == null);

        public Task<List<User>> GetUsersByEventAsync(Guid eventId) =>
            _context.EventParticipants
                .Where(p =>
                    p.EventId == eventId &&
                    p.LeftAt == null)
                .OrderBy(p => p.RegisterDate)
                .Select(p => p.User)
                .ToListAsync();

        public Task<List<Event>> GetEventsByActiveUser(Guid UserId)
        {
            return _context.EventParticipants
                .Where(p =>
                    p.UserId == UserId &&
                    p.LeftAt == null)
                .Select(p => p.Event)
                .ToListAsync();
        }

        public async Task<JoinEventStatus> JoinAsync(Guid eventId, Guid userId)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var ev = await _context.Events
                .FromSqlInterpolated(
                    $@"SELECT * FROM ""Events""
                    WHERE ""Id"" = {eventId} FOR UPDATE")
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (ev is null)
                return JoinEventStatus.EventNotFound;

            var participation = await _context.EventParticipants
                .FirstOrDefaultAsync(p =>
                    p.EventId == eventId &&
                    p.UserId == userId);

            if (participation is not null && participation.LeftAt is null)
            {
                return JoinEventStatus.AlreadyJoined;
            }

            var count = await _context.EventParticipants
                .CountAsync(p =>
                    p.EventId == eventId &&
                    p.LeftAt == null);

            if (count >= ev.MaxCapacity)
                return JoinEventStatus.Full;

            if (participation is not null)
            {
                participation.LeftAt = null;
                participation.RegisterDate = DateTime.UtcNow;
            }
            else
            {
                _context.EventParticipants.Add(
                    new EventParticipant
                    {
                        EventId = eventId,
                        UserId = userId,
                        RegisterDate = DateTime.UtcNow
                    });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return JoinEventStatus.Joined;
        }

        public async Task<bool> RemoveAsync(Guid eventId, Guid userId)
        {
            var participation = await _context.EventParticipants
                .FirstOrDefaultAsync(p =>
                    p.EventId == eventId &&
                    p.UserId == userId &&
                    p.LeftAt == null);

            if (participation is null)
                return false;

            participation.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}