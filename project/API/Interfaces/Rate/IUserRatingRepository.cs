using API.Models;

namespace API.Interfaces.Rate
{
    public interface IUserRatingRepository
    {
        Task<UserRating> AddAsync(UserRating userRating);
        Task<List<UserRating>> GetAllAsync();
        Task<List<UserRating>> GetRateByUser(Guid userId);
        Task UpdateRateUserAsync(UserRating userRating);
    }
}