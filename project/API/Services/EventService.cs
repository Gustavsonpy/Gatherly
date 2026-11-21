using API.Common;
using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Category;
using API.Interfaces.Event;
using API.Interfaces.Participant;
using API.Models;
using API.Repositories;
using static API.Interfaces.Participant.IEventParticipantRepository;

namespace API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEventParticipantRepository _participantRepository;
        private readonly ICategoryRepository _categoryRepository;

        public EventService(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            IEventParticipantRepository participantRepository,
            ICurrentUserService currentUserService,
            ICategoryRepository categoryRepository
            )
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _participantRepository = participantRepository;
            _currentUserService = currentUserService;
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<EventDTO>> CreateAsync(CreateEventDTO createEventDTO, Guid userId)
        {
            var errors = new List<string>();
            var title = createEventDTO.Title.Trim();

            if (string.IsNullOrWhiteSpace(title))
                errors.Add("O título é obrigatório.");

            if (createEventDTO.DateTimeStart <= DateTime.UtcNow)
                errors.Add("O evento deve começar no futuro.");

            if (createEventDTO.DateTimeEnd <= createEventDTO.DateTimeStart)
                errors.Add("O término deve ser depois do início");

            if (!IsOwnUploadUrl(createEventDTO.UrlImage, "events"))
                errors.Add("Imagem inválida.");

            if (await _categoryRepository.GetByIdAsync(createEventDTO.CategoryId) is null)
                errors.Add("Categoria não encontrada.");

            if (errors.Any())
                return Result<EventDTO>.Failure(errors);


            var newEvent = new Event
            {
                Title = createEventDTO.Title,
                Description = createEventDTO.Description,
                DateTimeStart = createEventDTO.DateTimeStart,
                DateTimeEnd = createEventDTO.DateTimeEnd,
                Localization = createEventDTO.Localization,
                MaxCapacity = createEventDTO.MaxCapacity,
                City = createEventDTO.City,
                Level = createEventDTO.Level,
                UrlImage = createEventDTO.UrlImage,
                PlaceName = createEventDTO.PlaceName,
                Street = createEventDTO.Street,
                District = createEventDTO.District,
                State = createEventDTO.State,
                Latitude = createEventDTO.Latitude,
                Longitude = createEventDTO.Longitude,
                UserId = userId,
                CategoryId = createEventDTO.CategoryId,
                Participants =
                {
                    new EventParticipant
                    {
                        UserId = userId,
                        RegisterDate = DateTime.UtcNow
                    }
                }
            };

            var created = await _eventRepository.AddAsync(newEvent);

            var resultDto = new EventDTO
            {
                Id = created.Id,
                Title = created.Title,
                CategoryId = created.CategoryId,
                Description = created.Description,
                DateTimeStart = created.DateTimeStart,
                DateTimeEnd = created.DateTimeEnd,
                Localization = created.Localization,
                MaxCapacity = created.MaxCapacity,
                City = created.City,
                Level = created.Level,
                UrlImage = created.UrlImage,
                PlaceName = created.PlaceName,
                Street = created.Street,
                District = created.District,
                State = created.State,
                Latitude = created.Latitude,
                Longitude = created.Longitude,
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

            var dto = new ReturnEventDTO
            {
                Id = eventId.Id,
                Title = eventId.Title,
                CategoryId = eventId.CategoryId,
                CategoryName = eventId.Category?.Name,
                Description = eventId.Description,
                DateTimeStart = eventId.DateTimeStart,
                DateTimeEnd = eventId.DateTimeEnd,
                Localization = eventId.Localization,
                MaxCapacity = eventId.MaxCapacity,
                City = eventId.City,
                Level = eventId.Level,
                UrlImage = eventId.UrlImage,
                PlaceName = eventId.PlaceName,
                Street = eventId.Street,
                District = eventId.District,
                State = eventId.State,
                Latitude = eventId.Latitude,
                Longitude = eventId.Longitude,
                RegisterDate = eventId.RegisterDate,
                UserId = eventId.UserId,
                CreatorName = eventId.User?.Name,
                CreatorPhoto = eventId.User?.UrlPhoto,
                ParticipantsCount = await _participantRepository.CountByEventAsync(id),
                IsParticipating = userId != Guid.Empty
                    && await _participantRepository.ExistsAsync(id, userId),
                Status = GetStatus(eventId)
            };

            return Result<ReturnEventDTO?>.Success(dto);
        }

        public async Task<Result<List<ReturnEventDTO>>> GetAllAsync()
        {
            var userId = _currentUserService.UserId;
            var events = await _eventRepository.GetAllAsync();

            return Result<List<ReturnEventDTO>>.Success(
                events.Select(e => ToReturnDto(e, userId)).ToList());
        }

        public async Task<Result<List<ReturnEventDTO>>> GetMyEventsAsync()
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<List<ReturnEventDTO>>.Failure("Usuário não autenticado.");

            var events = await _participantRepository.GetEventsByActiveUser(userId);

            return Result<List<ReturnEventDTO>>.Success(
                events.Select(e => ToReturnDto(e, userId)).ToList());
        }

        public async Task<Result<ReturnEventDTO>> UpdateAsync(Guid eventId, UpdateEventDTO dto)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<ReturnEventDTO>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<ReturnEventDTO>.Failure("Evento não encontrado.");

            if (ev.UserId != userId)
                return Result<ReturnEventDTO>.Failure("Apenas o organizador pode editar o evento.");

            if (ev.CanceledAt is not null)
                return Result<ReturnEventDTO>.Failure("Não é possível editar um evento cancelado.");

            if (ev.DateTimeStart <= DateTime.UtcNow)
                return Result<ReturnEventDTO>.Failure("Só é possível editar eventos que ainda não começaram.");

            var errors = new List<string>();

            var sameTitle = await _eventRepository.GetByTitle(dto.Title);
            if (sameTitle is not null && sameTitle.Id != eventId)
                errors.Add("Já existe um evento com esse nome");

            if (dto.DateTimeStart <= DateTime.UtcNow)
                errors.Add("A data de início deve ser futura");

            if (dto.DateTimeEnd <= dto.DateTimeStart)
                errors.Add("O término deve ser depois do início");

            var participantsCount = await _participantRepository.CountByEventAsync(eventId);
            if (dto.MaxCapacity < participantsCount)
                errors.Add($"O evento já tem {participantsCount} participantes, as vagas não podem ser menores que isso");

            if (errors.Any())
                return Result<ReturnEventDTO>.Failure(errors);

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.DateTimeStart = dto.DateTimeStart;
            ev.DateTimeEnd = dto.DateTimeEnd;
            ev.Localization = dto.Localization;
            ev.MaxCapacity = dto.MaxCapacity;
            ev.City = dto.City;
            ev.Level = dto.Level;
            ev.UrlImage = dto.UrlImage;
            ev.PlaceName = dto.PlaceName;
            ev.Street = dto.Street;
            ev.District = dto.District;
            ev.State = dto.State;
            ev.Latitude = dto.Latitude;
            ev.Longitude = dto.Longitude;
            ev.CategoryId = dto.CategoryId;

            await _eventRepository.UpdateAsync(ev);

            var updated = await GetByIdAsync(eventId);

            return updated.IsSuccess
                ? Result<ReturnEventDTO>.Success(updated.Value!)
                : Result<ReturnEventDTO>.Failure(updated.Errors);
        }

        public async Task<Result<bool>> DeleteAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.UserId != userId)
                return Result<bool>.Failure("Apenas o organizador pode excluir o evento.");

            if (ev.CanceledAt is null && ev.DateTimeStart <= DateTime.UtcNow)
                return Result<bool>.Failure("Não é possível excluir um evento que já começou ou aconteceu.");

            if (await _eventRepository.HasReportsOrRatingsAsync(eventId))
                return Result<bool>.Failure("Este evento possui denúncias ou avaliações e não pode ser excluído. Cancele o evento em vez disso.");

            await _eventRepository.DeleteAsync(ev);

            return Result<bool>.Success(true);
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

            return Result<List<ReturnEventDTO>>.Success(
                events.Select(e => ToReturnDto(e, userId)).ToList());
        }

        public async Task<Result<bool>> JoinAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.CanceledAt is not null)
                return Result<bool>.Failure("Este evento foi cancelado.");

            if (ev.DateTimeStart < DateTime.UtcNow && ev.DateTimeEnd > DateTime.UtcNow)
                return Result<bool>.Failure("Este evento já está em andamento.");

            if (ev.DateTimeEnd < DateTime.UtcNow)
                return Result<bool>.Failure("Este evento já aconteceu.");

            if (await _participantRepository.ExistsAsync(eventId, userId))
                return Result<bool>.Failure("Você já participa deste evento.");

            var count = await _participantRepository.CountByEventAsync(eventId);

            if (count >= ev.MaxCapacity)
                return Result<bool>.Failure("Evento lotado.");

            var status = await _participantRepository.JoinAsync(eventId, userId);

            return status switch
            {
                JoinEventStatus.Joined        => Result<bool>.Success(true),
                JoinEventStatus.AlreadyJoined => Result<bool>.Failure("Você já participa deste evento."),
                JoinEventStatus.Full          => Result<bool>.Failure("Evento lotado."),
                _                             => Result<bool>.Failure("Evento não encontrado.")
            };
        }

        public async Task<Result<bool>> LeaveAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.UserId == userId)
                return Result<bool>.Failure(
                    "O organizador não pode sair do próprio evento.");

            if (ev.DateTimeStart <= DateTime.UtcNow)
                return Result<bool>.Failure("Não é possível sair de um evento que já começou.");

            var removed = await _participantRepository.RemoveAsync(
                eventId,
                userId);

            return removed
                ? Result<bool>.Success(true)
                : Result<bool>.Failure(
                    "Você não participa deste evento.");
        }

        public async Task<Result<List<ParticipantDTO>>> GetParticipantsAsync(Guid eventId)
        {
            var dtos = await _participantRepository.GetParticipantsByEventAsync(eventId);

            return Result<List<ParticipantDTO>>.Success(dtos);
        }

        public async Task<Result<bool>> CancelAsync(Guid eventId)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                return Result<bool>.Failure("Usuário não autenticado.");

            var ev = await _eventRepository.GetByIdAsync(eventId);

            if (ev is null)
                return Result<bool>.Failure("Evento não encontrado.");

            if (ev.UserId != userId)
                return Result<bool>.Failure("Apenas o organizador pode cancelar o evento.");

            if (ev.CanceledAt is not null)
                return Result<bool>.Failure("Este evento já foi cancelado.");

            if (ev.DateTimeStart <= DateTime.UtcNow)
                return Result<bool>.Failure("Só é possível cancelar eventos que ainda não começaram.");

            ev.CanceledAt = DateTime.UtcNow;
            await _eventRepository.UpdateAsync(ev);

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<ReturnEventDTO>>> GetFutureEvents()
        {
            var userId = _currentUserService.UserId;
            
            if (userId == Guid.Empty)
                return Result<List<ReturnEventDTO>>.Failure("Usuário não autenticado.");

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return Result<List<ReturnEventDTO>>.Failure("Usuário não encontrado.");
            
            var events = await _eventRepository.GetFutureEvents();

            return Result<List<ReturnEventDTO>>.Success(events.Select(e => ToReturnDto(e, userId)).ToList());

        }

        private static ReturnEventDTO ToReturnDto(Event e, Guid userId) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name,
            DateTimeStart = e.DateTimeStart,
            DateTimeEnd = e.DateTimeEnd,
            Localization = e.Localization,
            MaxCapacity = e.MaxCapacity,
            City = e.City,
            Level = e.Level,
            UrlImage = e.UrlImage,
            PlaceName = e.PlaceName,
            Street = e.Street,
            District = e.District,
            State = e.State,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            RegisterDate = e.RegisterDate,
            UserId = e.UserId,
            CreatorName = e.User?.Name,
            CreatorPhoto = e.User?.UrlPhoto,
            ParticipantsCount = e.Participants.Count(p => p.LeftAt == null),
            IsParticipating = userId != Guid.Empty
                && e.Participants.Any(p =>
                    p.UserId == userId &&
                    p.LeftAt == null),
            Status = GetStatus(e),
        };

        private static bool IsOwnUploadUrl(string url, string folder){
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.AbsolutePath.StartsWith($"/uploads/{folder}/", StringComparison.OrdinalIgnoreCase);
        }
        private static string GetStatus(Event e) =>
            e.CanceledAt is not null ? "canceled"
            : DateTime.UtcNow < e.DateTimeStart ? "upcoming"
            : DateTime.UtcNow < e.DateTimeEnd ? "ongoing"
            : "done";
    }
}