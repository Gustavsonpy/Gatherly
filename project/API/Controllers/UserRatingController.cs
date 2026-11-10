using System.Security.Claims;
using API.DTO.Rate;
using API.Interfaces.Rate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/rate")]
    public class UserRatingController : ControllerBase
    {
        private readonly IUserRatingService _userRatingService;
        public UserRatingController(IUserRatingService userRatingService)
        {
            _userRatingService = userRatingService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserRatingDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _userRatingService.CreateRatingAsync(dto, userId);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }
    }
}