using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Models;

namespace API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEventParticipantRepository _participantRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            IEventParticipantRepository participantRepository,
            ICurrentUserService currentUserService, ILogger<EventService> logger
            )
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _participantRepository = participantRepository;
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

        public async Task<Result<ReturnEventDTO?>> GetByIdAsync(Guid id)
        {
            var eventId = await _eventRepository.GetByIdAsync(id);
            var userId = _currentUserService.UserId;

            if (eventId is null)
                return Result<ReturnEventDTO?>.Failure("Evento não encontrado.");

            var dto =  new ReturnEventDTO
            {
                Id = eventId.Id,
                Title = eventId.Title,
                CategoryId = eventId.CategoryId,
                CategoryName = eventId.Category?.Name,
                Description = eventId.Description,
                DateTime = eventId.DateTime,
                Date = eventId.DateTime.ToString("yyyy-MM-dd"),
                Time = eventId.DateTime.ToString("HH:mm"),
                Localization = eventId.Localization,
                MaxCapacity = eventId.MaxCapacity,
                City = eventId.City,
                Level = eventId.Level,
                UrlImage = eventId.UrlImage,
                RegisterDate = eventId.RegisterDate,
                UserId = eventId.UserId,
                User = eventId.User,
                ParticipantsCount = await _participantRepository.CountByEventAsync(id),
                IsParticipating = userId != Guid.Empty
                    && await _participantRepository.ExistsAsync(id, userId)
            };

            return Result<ReturnEventDTO?>.Success(dto);
        }

        public async Task<Result<List<ReturnEventDTO>>> GetAllAsync()
        {
            var events = await _eventRepository.GetAllAsync();

            var dtos = events.Select(e => new ReturnEventDTO
            {
                Id = e.Id,
                Title = e.Title,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
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
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
            }).ToList();

            return Result<List<ReturnEventDTO>>.Success(eventDTOs);
        }

        public async Task<Result<bool>> JoinAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.UserId == userId)
                return Result<bool>.Failure("Você é o organizador deste evento.");

            if (ev.DateTime < DateTime.UtcNow)
                return Result<bool>.Failure("Este evento já aconteceu.");

            if (await _participantRepository.ExistsAsync(eventId, userId))
                return Result<bool>.Failure("Você já participa deste evento.");

            var count = await _participantRepository.CountByEventAsync(eventId);

            if (count >= ev.MaxCapacity)
                return Result<bool>.Failure("Evento lotado.");

            await _participantRepository.AddAsync(new EventParticipant
            {
                EventId = eventId,
                UserId = userId,
                RegisterDate = DateTime.UtcNow
            });

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> LeaveAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var removed = await _participantRepository.RemoveAsync(eventId, userId);

            return removed
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Você não participa deste evento.");
        }

        public async Task<Result<List<ParticipantDTO>>> GetParticipantsAsync(Guid eventId)
        {
            var users = await _participantRepository.GetUsersByEventAsync(eventId);

            var dtos = users.Select(u => new ParticipantDTO
            {
                Id = u.Id,
                Name = u.Name,
                UrlPhoto = u.UrlPhoto
            }).ToList();

            return Result<List<ParticipantDTO>>.Success(dtos);
        }
    }
}