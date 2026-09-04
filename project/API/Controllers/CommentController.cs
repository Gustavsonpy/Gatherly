using API.DTO.Comment;
using API.Interfaces.Comment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCommentDTO dto)
        {
            var result = await _commentService.createAsync(dto);

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(dto);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _commentService.GetAllAsync();

            if(!result.IsSuccess)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Value);
        }
    }
}