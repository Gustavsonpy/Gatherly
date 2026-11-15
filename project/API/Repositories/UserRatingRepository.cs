using API.Data;
using API.DTO.Rate;
using API.Interfaces.Rate;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class UserRatingRepository : IUserRatingRepository
    {
        private readonly AppDbContext _context;
        public UserRatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserRating> AddAsync(UserRating userRating)
        {
            await _context.AddAsync(userRating);
            
            await _context.SaveChangesAsync();

            return userRating;
        }

        public async Task<List<UserRating>> GetAllAsync()
        {
            return await _context.UserRatings
                    .Include(ur => ur.Event)
                    .Include(ur => ur.RatedUser)
                    .Include(ur => ur.Reporter)
                    .ToListAsync();
        }

        public async Task<List<UserRating>> GetRateByUser(Guid userId)
        {
            return await _context.UserRatings
                    .Where(ur => ur.RatedUserId == userId)
                    .Include(ur => ur.Event)
                    .Include(ur => ur.RatedUser)
                    .Include(ur => ur.Reporter)
                    .ToListAsync();
        }

        public async Task<(double Average, int Count)> GetUserScoreAsync(Guid userId)
        {
            var query = _context.UserRatings.Where(r => r.RatedUserId == userId);
            var count = await query.CountAsync();
            var avg = count == 0 ? 0 : await query.AverageAsync(r => r.Rate);
            return (avg, count);
        }

        public async Task UpdateRateUserAsync(UserRating userRating)
        {
            _context.UserRatings.Update(userRating);
            await _context.SaveChangesAsync();
        }
    }
}