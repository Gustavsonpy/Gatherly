using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTO.Comment;
using API.Interfaces.Comment;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);

            await _context.SaveChangesAsync();

            return comment;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Comment>> GetAllAsync()
        {
            return await _context.Comments
                    .Include(c => c.User)
                    .OrderBy(c => c.RegisterDate)
                    .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<List<Comment?>> GetByText(string text)
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<List<Comment>> GetCommentsByEventAsync(Guid eventID) =>
            await _context.Comments
                .Where(c => c.EventId == eventID)
                .Include(c => c.User)
                .OrderBy(c => c.RegisterDate)
                .ToListAsync();

        public Task UpdateAsync(Comment informal_event)
        {
            throw new NotImplementedException();
        }
    }
}