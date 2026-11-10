using API.Common;
using API.DTO.Rate;

namespace API.Interfaces.Rate
{
    public interface IUserRatingService
    {
        Task<Result<UserRatingDTO>> CreateRatingAsync(CreateUserRatingDTO userRatingDTO, Guid reporterId);
        Task<List<UserRatingDTO>> GetAll();        
        Task<List<UserRatingDTO>> GetRatingByUser(Guid userId);        
        Task<List<UserRatingDTO>> UpdateUserRating(UserRatingDTO userRatingDTO);        
    }
}