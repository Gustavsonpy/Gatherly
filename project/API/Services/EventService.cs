using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Event;
using API.Models;

namespace API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, IUserRepository userRepository, ICurrentUserService currentUserService, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<EventDTO>> CreateAsync(CreateEventDTO createEventDTO, Guid userId)
        {
            var errors = new List<string>();

            var existedEvent = await _eventRepository.GetByTitle(createEventDTO.Title);

            if(existedEvent is not null)
                errors.Add("Already exists an event with this title");

            if(errors.Any())
                return Result<EventDTO>.Failure(errors);

            var newEvent = new Event
            {
                Title = createEventDTO.Title,
                Description = createEventDTO.Description,
                DateTime = createEventDTO.DateTime,
                Localization = createEventDTO.Localization,
                MaxCapacity = createEventDTO.MaxCapacity,
                City = createEventDTO.City,
                Level = createEventDTO.Level,
                UrlImage = createEventDTO.UrlImage,
                UserId = userId,
                CategoryId = createEventDTO.CategoryId
            };

            var created = await _eventRepository.AddAsync(newEvent);

            var resultDto = new EventDTO
            {
                Id = created.Id,
                Title = created.Title,
                CategoryId = created.CategoryId,
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

        public async Task<Result<List<ReturnEventDTO>>> GetAllAsync()
        {
            var events = await _eventRepository.GetAllAsync();

            var dtos = events.Select(e => new ReturnEventDTO
            {
                Id = e.Id,
                Title = e.Title,
                CategoryId = e.CategoryId,
                Description = e.Description,
                DateTime = e.DateTime,
                Date = e.DateTime.ToString("yyyy-MM-dd"),
                Time = e.DateTime.ToString("HH:mm"),
                Localization = e.Localization,
                MaxCapacity = e.MaxCapacity,
                City = e.City,
                Level = e.Level,
                UrlImage = e.UrlImage,
                RegisterDate = e.RegisterDate,
                UserId = e.UserId,
                User = e.User
            }).ToList();

            return Result<List<ReturnEventDTO>>.Success(dtos);
        }

        public Task<Result<EventDTO>> UpdateTitleAsync(string title)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<List<ReturnEventDTO>>> GetMyCityEventsAsync()
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<List<ReturnEventDTO>>.Failure("Usuário não autenticado.");

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return Result<List<ReturnEventDTO>>.Failure("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(user.City))
                return Result<List<ReturnEventDTO>>.Failure("Usuário não possui cidade cadastrada.");

            var events = await _eventRepository.GetByCityAsync(user.City);

            var eventDTOs = events.Select(e => new ReturnEventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                DateTime = e.DateTime,
                Date = e.DateTime.ToString("yyyy-MM-dd"),
                Time = e.DateTime.ToString("HH:mm"),
                Localization = e.Localization,
                MaxCapacity = e.MaxCapacity,
                City = e.City,
                Level = e.Level,
                UrlImage = e.UrlImage,
                RegisterDate = e.RegisterDate,
                UserId = e.UserId,
                CategoryId = e.CategoryId
            }).ToList();

            return Result<List<ReturnEventDTO>>.Success(eventDTOs);
        }
    }
}