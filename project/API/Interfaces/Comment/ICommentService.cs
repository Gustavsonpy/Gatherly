using API.Common;
using API.DTO.Comment;

namespace API.Interfaces.Comment
{
    public interface ICommentService
    {
        Task<Result<CommentDTO>> createAsync(CreateCommentDTO commentDTO);
        Task<Result<CommentDTO?>> GetByIdAsync(int id);
        Task<Result<List<CommentDTO>>> GetAllAsync();
        Task<Result<CommentDTO>> UpdateCommentAsync(string text);
    }
}