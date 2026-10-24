
using API.Common;
using API.DTO.Comment;
using API.Interfaces;
using API.Interfaces.Comment;
using API.Interfaces.Event;
using API.Models;

namespace API.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;
        public CommentService(ICommentRepository repository, ICurrentUserService currentUser, IUserRepository userRepository)
        {
            _commentRepository = repository;
            _currentUser = currentUser;
            _userRepository = userRepository;
        }
        public async Task<Result<CommentDTO>> createAsync(CreateCommentDTO commentDTO)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                return Result<CommentDTO>.Failure("Usuário não autenticado");

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return Result<CommentDTO>.Failure("Usuário não encontrado!");

            var newComment = new Comment
            {
                Text = commentDTO.Text,
                EventId = commentDTO.EventId,
                UserId = userId,
                RegisterDate = DateTime.UtcNow
            };

            var created = await _commentRepository.AddAsync(newComment);

            var newComentDto = new CommentDTO
            {
                Text = created.Text,
                EventId = created.EventId,
                UserId = created.UserId,
                CommentUserName = user.Name,
                UserPhoto = user.UrlPhoto,
                RegisterDate = DateTime.UtcNow
            };

            return Result<CommentDTO>.Success(newComentDto);
        }

        public async Task<Result<List<CommentDTO>>> GetAllAsync()
        {
            var comments = await _commentRepository.GetAllAsync();

            var dtos = comments.Select(c => new CommentDTO
            {
                Id = c.Id,
                Text = c.Text,
                RegisterDate = c.RegisterDate,
                EventId = c.EventId,
                UserId = c.UserId,
                CommentUserName = c.User.Name,
                UserPhoto = c.User.UrlPhoto

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

        public async Task<Result<List<CommentDTO>>> GetCommentsByEvent(Guid eventId)
        {
            var comments = await _commentRepository.GetCommentsByEventAsync(eventId);

            var dtos = comments.Select(c => new CommentDTO
            {
                Id = c.Id,
                Text = c.Text,
                RegisterDate = c.RegisterDate,
                EventId = c.EventId,
                UserId = c.UserId,
                CommentUserName = c.User.Name,
                UserPhoto = c.User.UrlPhoto,
            }).ToList();

            return Result<List<CommentDTO>>.Success(dtos);
        }
    }
}