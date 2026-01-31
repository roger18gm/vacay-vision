using Microsoft.EntityFrameworkCore;
using juveApp.Data;
using juveApp.Models;

namespace juveApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user by ID with role information
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Get all vacations created by a specific user
        /// </summary>
        public async Task<List<Vacation>> GetUserVacationsAsync(int userId)
        {
            return await _context.Vacations
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get user's approved community submissions count
        /// </summary>
        public async Task<int> GetUserApprovedSubmissionsCountAsync(int userId)
        {
            return await _context.CommunityRequests
                .Where(cr => cr.UserId == userId && cr.Status.ToLower() == "approved")
                .CountAsync();
        }

        /// <summary>
        /// Create support feedback from user
        /// </summary>
        public async Task<SupportFeedback> CreateFeedbackAsync(int userId, string feedbackType, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Feedback message cannot be empty");

            var feedback = new SupportFeedback
            {
                SubmittedById = userId,
                FeedbackType = feedbackType,
                Message = message,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportFeedback.Add(feedback);
            await _context.SaveChangesAsync();

            return feedback;
        }

        /// <summary>
        /// Check if user exists
        /// </summary>
        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Get user statistics for profile display
        /// </summary>
        public async Task<UserStats> GetUserStatsAsync(int userId)
        {
            var stats = new UserStats
            {
                TotalVacations = await _context.Vacations.CountAsync(v => v.UserId == userId),
                TotalComments = await _context.VacationComments.CountAsync(c => c.UserId == userId),
                ApprovedSubmissions = await _context.CommunityRequests
                    .CountAsync(cr => cr.UserId == userId && cr.Status.ToLower() == "approved"),
                PendingSubmissions = await _context.CommunityRequests
                    .CountAsync(cr => cr.UserId == userId && cr.Status.ToLower() == "pending")
            };

            return stats;
        }
    }

    /// <summary>
    /// User statistics for profile page
    /// </summary>
    public class UserStats
    {
        public int TotalVacations { get; set; }
        public int TotalComments { get; set; }
        public int ApprovedSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
    }
}
