using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Event;
using API.Interfaces;
using API.Interfaces.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/event")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IImageStorage _imageStorage;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, IImageStorage imageStorage, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateEventDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _eventService.CreateAsync(dto, userId);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _eventService.GetAllAsync();

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var result = await _eventService.GetByIdAsync(id);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("my-city")]
        public async Task<IActionResult> GetMyCityEvents()
        {
            var result = await _eventService.GetMyCityEventsAsync();

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("upload-image")]
        [RequestSizeLimit(ImageFileValidator.MaxSizeBytes)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var validationError = ImageFileValidator.Validate(file);

            if (validationError is not null)
                return BadRequest(new { errors = new[] { validationError } });

            var url = await _imageStorage.SaveAsync(file, "events");

            return Ok(new { url });
        }

        [Authorize]
        [HttpPost("{id:guid}/join")]
        public async Task<IActionResult> Join(Guid id)
        {
            var result = await _eventService.JoinAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:guid}/join")]
        public async Task<IActionResult> Leave(Guid id)
        {
            var result = await _eventService.LeaveAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        [Authorize]
        [HttpGet("{id:guid}/participants")]
        public async Task<IActionResult> GetParticipants(Guid id)
        {
            var result = await _eventService.GetParticipantsAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyEvents()
        {
            var result = await _eventService.GetMyEventsAsync();

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _eventService.CancelAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }
    }
}