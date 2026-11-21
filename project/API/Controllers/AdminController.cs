using System.Security.Claims;
using API.Common;
using API.DTO.Admin;
using API.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = AdminRole)]
    public class AdminController : ControllerBase
    {
        public const string AdminRole = "Admin";

        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private Guid AdminId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        private IActionResult FromResult(Result<bool> result) =>
            result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Errors });

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats() => Ok(await _adminService.GetStatsAsync());

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] string? status)
        {
            var result = await _adminService.GetReportsAsync(status);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
        }

        [HttpPatch("reports/{id:guid}/status")]
        public async Task<IActionResult> UpdateReportStatus(Guid id, [FromBody] UpdateReportStatusDTO dto) =>
            FromResult(await _adminService.UpdateReportStatusAsync(AdminId, id, dto.Status));

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search) =>
            Ok(await _adminService.GetUsersAsync(search));

        [HttpPatch("users/{id:guid}/suspended")]
        public async Task<IActionResult> SetSuspended(Guid id, [FromBody] UpdateFlagDTO dto) =>
            FromResult(await _adminService.SetUserSuspendedAsync(AdminId, id, dto.Value));

        [HttpPatch("users/{id:guid}/admin")]
        public async Task<IActionResult> SetAdmin(Guid id, [FromBody] UpdateFlagDTO dto) =>
            FromResult(await _adminService.SetUserAdminAsync(AdminId, id, dto.Value));

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] string? search) =>
            Ok(await _adminService.GetEventsAsync(search));

        [HttpPost("events/{id:guid}/cancel")]
        public async Task<IActionResult> CancelEvent(Guid id) =>
            FromResult(await _adminService.CancelEventAsync(AdminId, id));

        [HttpGet("comments")]
        public async Task<IActionResult> GetComments([FromQuery] string? search) =>
            Ok(await _adminService.GetCommentsAsync(search));

        [HttpDelete("comments/{id:guid}")]
        public async Task<IActionResult> DeleteComment(Guid id) =>
            FromResult(await _adminService.DeleteCommentAsync(AdminId, id));
    }
}
