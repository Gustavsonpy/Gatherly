
using API.Common;
using API.DTO.Comment;
using API.Interfaces;
using API.Interfaces.Comment;
using API.Models;

namespace API.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICurrentUserService _currentUser;

        public CommentService(ICommentRepository repository, ICurrentUserService currentUser)
        {
            _commentRepository = repository;
            _currentUser = currentUser;
        }
        public async Task<Result<CommentDTO>> createAsync(CreateCommentDTO commentDTO)
        {
            var newComment = new Comment
            {
                Text = commentDTO.Text,
                EventId = commentDTO.EventId,
                UserId = _currentUser.UserId
            };

            var created = await _commentRepository.AddAsync(newComment);

            var newComentDto = new CommentDTO
            {
                Text = created.Text,
                EventId = created.EventId,
                UserId = created.UserId
            };

            return Result<CommentDTO>.Success(newComentDto);
        }

        public async Task<Result<List<CommentDTO>>> GetAllAsync()
        {
            var comments = await _commentRepository.GetAllAsync();

            var dtos = comments.Select(c => new CommentDTO
            {
                Text = c.Text,
                EventId = c.EventId,
                UserId = c.UserId
            }).ToList();

            return Result<List<CommentDTO>>.Success(dtos);
        }

        public Task<Result<CommentDTO?>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CommentDTO>> UpdateCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
}