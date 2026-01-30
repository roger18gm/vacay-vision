using Microsoft.EntityFrameworkCore;
using juveApp.Data;
using juveApp.Models;
using juveApp.Models.ViewModels;

namespace juveApp.Services
{
    public class CommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all comments for a specific vacation with User navigation
        /// </summary>
        public async Task<List<VacationComment>> GetCommentsByVacationAsync(int vacationId)
        {
            return await _context.VacationComments
                .Include(c => c.User)
                .Where(c => c.VacationId == vacationId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Add a new comment to a vacation
        /// </summary>
        public async Task<VacationComment> AddCommentAsync(AddCommentViewModel model, int userId)
        {
            var comment = new VacationComment
            {
                VacationId = model.VacationId,
                UserId = userId,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.VacationComments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with User navigation for display
            await _context.Entry(comment)
                .Reference(c => c.User)
                .LoadAsync();

            return comment;
        }

        /// <summary>
        /// Delete a comment (with ownership verification)
        /// </summary>
        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.VacationComments
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);

            if (comment == null)
                return false;

            _context.VacationComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Check if a user owns a specific comment
        /// </summary>
        public async Task<bool> UserOwnsCommentAsync(int commentId, int userId)
        {
            return await _context.VacationComments
                .AnyAsync(c => c.CommentId == commentId && c.UserId == userId);
        }
    }
}
