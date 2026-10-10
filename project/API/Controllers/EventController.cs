using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTO.Event;
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
        private readonly ILogger<EventController> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" }; 
        private const long MaxImageSize = 5 * 1024 * 1024;

        public EventController(IEventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _eventService.GetAllAsync();

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
        [RequestSizeLimit(MaxImageSize)]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { errors = new[] { "Nenhum arquivo enviado" } });

            if (file.Length > MaxImageSize)
                return BadRequest(new { errors = new[] { "A imagem deve ter no máximo 5 MB" } });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
                return BadRequest(new { errors = new[] { "Formato de imagem inválido" } });

            var folder = Path.Combine(env.WebRootPath, "uploads", "events");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/events/{fileName}";

            return Ok(new { url });
        }
    }
}