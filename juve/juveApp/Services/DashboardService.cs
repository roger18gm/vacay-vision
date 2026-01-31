using Microsoft.EntityFrameworkCore;
using juveApp.Data;
using juveApp.Models;

namespace juveApp.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all pending community submission requests
        /// </summary>
        public async Task<List<CommunityRequest>> GetPendingRequestsAsync()
        {
            return await _context.CommunityRequests
                .Include(cr => cr.User)
                .Include(cr => cr.Vacation)
                .Where(cr => cr.Status == "pending")
                .OrderBy(cr => cr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all rejected community submission requests
        /// </summary>
        public async Task<List<CommunityRequest>> GetRejectedRequestsAsync()
        {
            return await _context.CommunityRequests
                .Include(cr => cr.User)
                .Include(cr => cr.Vacation)
                .Where(cr => cr.Status == "rejected")
                .OrderByDescending(cr => cr.UpdatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Update the status of a community request (approve/reject)
        /// </summary>
        public async Task<bool> UpdateCommunityRequestStatusAsync(int requestId, string status, string? rejectionReason = null)
        {
            var request = await _context.CommunityRequests.FindAsync(requestId);
            if (request == null) return false;

            request.Status = status;
            request.RejectionReason = rejectionReason;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Search for a user by username (exact or partial match)
        /// </summary>
        public async Task<List<User>> SearchUsersByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Username.Contains(username))
                .OrderBy(u => u.Username)
                .Take(20) // Limit results
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Create a new headline message
        /// </summary>
        public async Task<Headline> CreateHeadlineAsync(string content, int adminUserId)
        {
            var headline = new Headline
            {
                Message = content,
                CreatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Headlines.Add(headline);
            await _context.SaveChangesAsync();

            return headline;
        }

        /// <summary>
        /// Get recent headlines
        /// </summary>
        public async Task<List<Headline>> GetRecentHeadlinesAsync(int count = 5)
        {
            return await _context.Headlines
                .Include(h => h.Creator)
                .OrderByDescending(h => h.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Delete a headline by ID
        /// </summary>
        public async Task<bool> DeleteHeadlineAsync(int headlineId)
        {
            var headline = await _context.Headlines.FindAsync(headlineId);
            if (headline == null) return false;

            _context.Headlines.Remove(headline);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get statistics for dashboard overview
        /// </summary>
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats
            {
                PendingRequestsCount = await _context.CommunityRequests.CountAsync(cr => cr.Status == "pending"),
                RejectedRequestsCount = await _context.CommunityRequests.CountAsync(cr => cr.Status == "rejected"),
                ApprovedRequestsCount = await _context.CommunityRequests.CountAsync(cr => cr.Status == "approved"),
                TotalUsersCount = await _context.Users.CountAsync(),
                TotalVacationsCount = await _context.Vacations.CountAsync()
            };

            return stats;
        }

        /// <summary>
        /// Get recent support feedback submissions
        /// </summary>
        public async Task<List<SupportFeedback>> GetRecentFeedbackAsync(int count = 10)
        {
            return await _context.SupportFeedback
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get pending support feedback
        /// </summary>
        public async Task<List<SupportFeedback>> GetPendingFeedbackAsync()
        {
            return await _context.SupportFeedback
                .Include(f => f.User)
                .Where(f => f.Status == "pending")
                .OrderBy(f => f.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Update feedback status
        /// </summary>
        public async Task<bool> UpdateFeedbackStatusAsync(int feedbackId, string status)
        {
            var feedback = await _context.SupportFeedback.FindAsync(feedbackId);
            if (feedback == null) return false;

            feedback.Status = status;
            if (status == "resolved")
            {
                feedback.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>
    /// Statistics for dashboard overview
    /// </summary>
    public class DashboardStats
    {
        public int PendingRequestsCount { get; set; }
        public int RejectedRequestsCount { get; set; }
        public int ApprovedRequestsCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int TotalVacationsCount { get; set; }
    }
}
