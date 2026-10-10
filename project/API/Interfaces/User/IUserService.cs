using API.Common;
using API.DTO;
using API.DTO.User;

namespace API.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDTO>> CreateAsync(CreateUserDTO createUserDTO);
        Task<Result<UserPublicDTO?>> GetByIdAsync(Guid id);
        Task<Result<List<UserPublicDTO>>> GetAllAsync();
        Task<Result<UserDTO>> UpdateEmailAsync(string email);
    }
}