using API.Models;

namespace API.Interfaces.Participant
{
    public interface IEventParticipantRepository
    {
        public enum JoinEventStatus { Joined, EventNotFound, AlreadyJoined, Full }
        Task<bool> ExistsAsync(Guid eventId, Guid userId);
        Task<int> CountByEventAsync(Guid eventId);
        Task<List<User>> GetUsersByEventAsync(Guid eventId);
        Task<JoinEventStatus> JoinAsync(Guid eventId, Guid userId);
        Task<bool> RemoveAsync(Guid eventId, Guid userId);
    }
}