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

        public async Task<Result<UserRatingDTO>> CreateRatingAsync(CreateUserRatingDTO userRatingDTO, Guid reporterId)
        {
            if(reporterId == userRatingDTO.RatedUserId)
            {
                return Result<UserRatingDTO>.Failure("Você não pode avaliar a si mesmo!");
            }

            var newUserRating = new UserRating
            {
                Rate = userRatingDTO.Rate,
                Description = userRatingDTO.Description,
                ReporterId = reporterId,
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

        public Task<List<UserRatingDTO>> GetRatingByUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserRatingDTO>> UpdateUserRating(UserRatingDTO userRatingDTO)
        {
            throw new NotImplementedException();
        }
    }
}