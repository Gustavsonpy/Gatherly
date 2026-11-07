using API.DTO.Report;
using API.Interfaces.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class UserReportController : Controller
    {
        private readonly IUserReportService _userReportService;
        public UserReportController(IUserReportService userReportService)
        {
            _userReportService = userReportService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateReportDTO dto)
        {
            var result = await _userReportService.CreateReport(dto);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }
    }
}