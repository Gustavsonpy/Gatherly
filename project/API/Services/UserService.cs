using System.Globalization;
using API.Common;
using API.DTO;
using API.DTO.User;
using API.Interfaces;
using API.Models;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result<UserDTO>> CreateAsync(CreateUserDTO userDTO)
        {
            var fieldErrors = new Dictionary<string, List<string>>();

            var existedUser = await _userRepository.GetByEmail(userDTO.Email);

            if(existedUser is not null)
                fieldErrors["email"] = new List<string> { "Já existe uma conta com esse e-mail." };

            if(userDTO.Password.Length < 6)
                fieldErrors["password"] = new List<string> { "A senha precisa ter 6 caracteres ou mais." };

            if (userDTO.Birthday.Date > DateTime.Today)
                fieldErrors["birthDate"] = new List<string> { "A data não pode ultrapassar o dia de hoje" };

            if(fieldErrors.Any())
                return Result<UserDTO>.Failure(fieldErrors);

            var newUser = new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Birthday = DateTime.SpecifyKind(userDTO.Birthday, DateTimeKind.Utc),
                HashPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                RegisterDate = DateTime.UtcNow,
                IsSuspended = false
            };

            var created = await _userRepository.AddAsync(newUser);

            return Result<UserDTO>.Success(ToDto(created));

        }
        
        public async Task<Result<UserDTO>> GetMeAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result<UserDTO>.Failure("Usuário não encontrado");

            return Result<UserDTO>.Success(ToDto(user));
        }

        public async Task<Result<UserDTO>> UpdateMeAsync(Guid id, UpdateUserDTO dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result<UserDTO>.Failure("Usuário não encontrado");

            var fieldErrors = new Dictionary<string, List<string>>();
            var name = (dto.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                fieldErrors["name"] = new List<string> { "Informe seu nome." };

            if (dto.Birthday == default)
                fieldErrors["birthday"] = new List<string> { "Informe sua data de nascimento." };
            else if (dto.Birthday.Date > DateTime.Today)
                fieldErrors["birthday"] = new List<string> { "A data de nascimento não pode ser no futuro." };

            if (fieldErrors.Count > 0)
                return Result<UserDTO>.Failure(fieldErrors);

            user.Name = name;
            user.Birthday = DateTime.SpecifyKind(dto.Birthday, DateTimeKind.Utc);
            user.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            user.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();

            await _userRepository.UpdateAsync(user);

            return Result<UserDTO>.Success(ToDto(user));
        }

        public async Task<Result<UserDTO>> UpdatePhotoAsync(Guid id, string urlPhoto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result<UserDTO>.Failure("Usuário não encontrado");

            user.UrlPhoto = urlPhoto;

            await _userRepository.UpdateAsync(user);

            return Result<UserDTO>.Success(ToDto(user));
        }

        private static UserDTO ToDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Birthday = user.Birthday,
            Description = user.Description,
            City = user.City,
            UrlPhoto = user.UrlPhoto,
            IsSuspended = user.IsSuspended,
            IsAdmin = user.IsAdmin,
            RegisterDate = user.RegisterDate
        };

        public async Task<Result<UserPublicDTO?>> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
            {
                return Result<UserPublicDTO?>.Failure("Usuário não encontrado");
            }

            var dto = new UserPublicDTO
            {
                Id = user.Id,
                Name = user.Name,
                Description = user.Description,
                City = user.City,
                UrlPhoto = user.UrlPhoto
            };

            return Result<UserPublicDTO?>.Success(dto);
        }

        public async Task<Result<List<UserPublicDTO>>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            var dtos = users.Select(u => new UserPublicDTO
            {
                Id = u.Id,
                Name = u.Name,
                Description = u.Description,
                City = u.City,
                UrlPhoto = u.UrlPhoto
            }).ToList();

            return Result<List<UserPublicDTO>>.Success(dtos);
        }

        public Task<Result<UserDTO>> UpdateEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}