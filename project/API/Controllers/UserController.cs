using System.Security.Claims;
using API.Common;
using API.DTO;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
namespace API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IImageStorage _imageStorage;

        public UserController(IUserService userService, IImageStorage imageStorage)
        {
            _userService = userService;
            _imageStorage = imageStorage;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserDTO dto)
        {
            var result = await _userService.CreateAsync(dto);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors, fieldErrors = result.FieldErrors });

            return Ok(result.Value);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }
 
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
 
            var result = await _userService.GetMeAsync(userId);
 
            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });
 
            return Ok(result.Value);
        }
 
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UpdateUserDTO dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
 
            var result = await _userService.UpdateMeAsync(userId, dto);
 
            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors, fieldErrors = result.FieldErrors });
 
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("me/photo")]
        [RequestSizeLimit(ImageFileValidator.MaxSizeBytes)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var validationError = ImageFileValidator.Validate(file);

            if (validationError is not null)
                return BadRequest(new { errors = new[] { validationError } });

            var current = await _userService.GetMeAsync(userId);

            if (!current.IsSuccess)
                return BadRequest(new { errors = current.Errors });

            var newUrl = await _imageStorage.SaveAsync(file, "users");

            var result = await _userService.UpdatePhotoAsync(userId, newUrl);

            if (!result.IsSuccess)
            {
                await _imageStorage.DeleteAsync(newUrl);
                return BadRequest(new { errors = result.Errors });
            }

            await _imageStorage.DeleteAsync(current.Value!.UrlPhoto);

            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetByIdAsync(id);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }
 
        private bool TryGetUserId(out Guid userId)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
            return Guid.TryParse(claim, out userId);
        }
    }
}