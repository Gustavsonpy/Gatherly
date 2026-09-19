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

            var resultDto = new UserDTO
            {
                Id = created.Id,
                Name = created.Name,
                Email = created.Email,
                Birthday = created.Birthday,
                Description = created.Description,
                City = created.City,
                UrlPhoto = created.UrlPhoto,
                IsSuspended = created.IsSuspended,
                IsAdmin = created.IsAdmin,
                RegisterDate = created.RegisterDate
            };

            return Result<UserDTO>.Success(resultDto);

        }

        public Task<Result<UserDTO?>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
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