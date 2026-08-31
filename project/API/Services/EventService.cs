using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Event;
using API.Interfaces.Event;
using API.Models;

namespace API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<Result<EventDTO>> CreateAsync(EventDTO eventDTO)
        {
            var errors = new List<string>();

            var existedEvent = await _eventRepository.GetByTitle(eventDTO.Title);

            if(existedEvent is not null)
                errors.Add("Already exists an event with this title");

            if(errors.Any())
                return Result<EventDTO>.Failure(errors);

            var newEvent = new Event
            {
                Title = eventDTO.Title,
                Category = eventDTO.Category,
                Description = eventDTO.Description,
                DateTime = eventDTO.DateTime,
                Localization = eventDTO.Localization,
                MaxCapacity = eventDTO.MaxCapacity,
                Level = eventDTO.Level,
                UrlImage = eventDTO.UrlImage
            };

            var created = await _eventRepository.AddAsync(newEvent);

            var resultDto = new EventDTO
            {
                Id = created.Id,
                Title = created.Title,
                Category = created.Category,
                Description = created.Description,
                DateTime = created.DateTime,
                Localization = created.Localization,
                MaxCapacity = created.MaxCapacity,
                City = created.City,
                Level = created.Level,
                UrlImage = created.UrlImage,
                RegisterDate = created.RegisterDate,
                UserId = created.UserId,
                User = created.User
            };

            return Result<EventDTO>.Success(resultDto);
        }

        public Task<Result<EventDTO?>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<List<EventDTO>>> GetAllAsync()
        {
            var events = await _eventRepository.GetAllAsync();

            var dtos = events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Category = e.Category,
                Description = e.Description,
                DateTime = e.DateTime,
                Localization = e.Localization,
                MaxCapacity = e.MaxCapacity,
                City = e.City,
                Level = e.Level,
                UrlImage = e.UrlImage,
                RegisterDate = e.RegisterDate,
                UserId = e.UserId,
                User = e.User
            }).ToList();

            return Result<List<EventDTO>>.Success(dtos);
        }

        public Task<Result<EventDTO>> UpdateTitleAsync(string title)
        {
            throw new NotImplementedException();
        }
    }
}