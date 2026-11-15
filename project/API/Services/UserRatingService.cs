using API.Common;
using API.DTO.Rate;
using API.Interfaces.Rate;
using API.Models;

namespace API.Services
{
    public class UserRatingService : IUserRatingService
    {
        private readonly IUserRatingRepository _userRatingRepository;

        public UserRatingService(IUserRatingRepository userRatingRepository)
        {
            _userRatingRepository = userRatingRepository;
        }

        public async Task<Result<UserRatingDTO>> CreateRatingAsync(CreateUserRatingDTO userRatingDTO)
        {
            if (userRatingDTO.ReporterId == userRatingDTO.RatedUserId)
                return Result<UserRatingDTO>.Failure("Você não pode avaliar a si mesmo.");

            var newUserRating = new UserRating
            {
                Rate = userRatingDTO.Rate,
                Description = userRatingDTO.Description,
                ReporterId = userRatingDTO.ReporterId,
                RatedUserId = userRatingDTO.RatedUserId,
                EventId = userRatingDTO.EventId
            };

            var created = await _userRatingRepository.AddAsync(newUserRating);

            var resultDto = new UserRatingDTO
            {
                Id = created.Id,
                Rate = created.Rate,
                Description = created.Description,
                ReporterId = created.ReporterId,
                RatedUserId = created.RatedUserId,
                EventId = created.EventId
            };

            return Result<UserRatingDTO>.Success(resultDto);
        }

        public Task<List<UserRatingDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserRatingDTO>> GetRatingsReceivedByUser(Guid userId)
        {
            var ratings = await _userRatingRepository.GetRateByUser(userId);
            return ratings.Select(MapToDto).ToList();
        }

        public async Task<List<UserRatingDTO>> GetRatingByUser(Guid userId)
        {
            var ratings = await _userRatingRepository.GetRateByUser(userId);

            return ratings
                .OrderByDescending(r => r.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<UserScoreDTO> GetUserScore(Guid userId)
        {
            var (average, count) = await _userRatingRepository.GetUserScoreAsync(userId);

            return new UserScoreDTO
            {
                UserId = userId,
                Average = average,
                Count = count
            };
        }

        public Task<List<UserRatingDTO>> UpdateUserRating(UserRatingDTO userRatingDTO)
        {
            throw new NotImplementedException();
        }
        
        private static UserRatingDTO MapToDto(UserRating rating) => new UserRatingDTO
        {
            Id = rating.Id,
            Rate = rating.Rate,
            Description = rating.Description,
            ReporterId = rating.ReporterId,
            RatedUserId = rating.RatedUserId,
            EventId = rating.EventId
        };
    }
}