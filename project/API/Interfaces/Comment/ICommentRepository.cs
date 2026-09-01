using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentModel = API.Models.Comment;

namespace API.Interfaces.Comment
{
    public interface ICommentRepository
    {
        Task<CommentModel?> GetByIdAsync(int id);
        Task<List<CommentModel?>> GetByText(string text);
        Task<List<CommentModel>> GetAllAsync();
        Task<CommentModel> AddAsync(CommentModel comment);
        Task UpdateAsync(CommentModel comment);
        Task DeleteAsync(int id);
    }
}