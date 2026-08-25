using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Event;

namespace API.Interfaces.Event
{
    public interface IEventService
    {
        Task<Result<EventDTO>> CreateAsync(EventDTO eventDTO);
        Task<Result<EventDTO?>> GetByIdAsync(int id);
        Task<Result<List<EventDTO>>> GetAllAsync();
        Task<Result<EventDTO>> UpdateTitleAsync(string title);
    }
}