using API.Models;

namespace API.Interfaces.Participant
{
    public interface IEventParticipantRepository
    {
        Task<bool> ExistsAsync(Guid eventId, Guid userId);
        Task<int> CountByEventAsync(Guid eventId);
        Task<List<User>> GetUsersByEventAsync(Guid eventId);
        Task AddAsync(EventParticipant participant);
        Task<bool> RemoveAsync(Guid eventId, Guid userId);
    }
}