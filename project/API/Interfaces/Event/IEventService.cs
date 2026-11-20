using API.Common;
using API.DTO.Event;

namespace API.Interfaces.Event
{
    public interface IEventService
    {
        Task<Result<EventDTO>> CreateAsync(CreateEventDTO eventDTO, Guid userId);
        Task<Result<ReturnEventDTO?>> GetByIdAsync(Guid id);
        Task<Result<List<ReturnEventDTO>>> GetMyCityEventsAsync();
        Task<Result<List<ReturnEventDTO>>> GetAllAsync();
        Task<Result<List<ReturnEventDTO>>> GetMyEventsAsync();
        Task<Result<ReturnEventDTO>> UpdateAsync(Guid eventId, UpdateEventDTO dto);
        Task<Result<bool>> DeleteAsync(Guid eventId);
        Task<Result<bool>> JoinAsync(Guid eventId);
        Task<Result<bool>> LeaveAsync(Guid eventId);
        Task<Result<List<ParticipantDTO>>> GetParticipantsAsync(Guid eventId);
        Task<Result<List<ReturnEventDTO>>> GetFutureEvents();
        Task<Result<bool>> CancelAsync(Guid eventId);
    }
}