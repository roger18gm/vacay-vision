using Microsoft.EntityFrameworkCore;
using juveApp.Data;
using juveApp.Models;

namespace juveApp.Services
{
    public class CommunityService
    {
        private readonly AppDbContext _context;

        public CommunityService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all approved vacation submissions for community page
        /// </summary>
        public async Task<List<CommunityRequest>> GetApprovedVacationsAsync()
        {
            return await _context.CommunityRequests
                .Include(cr => cr.Vacation)
                    .ThenInclude(v => v!.User)
                .Where(cr => cr.Status.ToUpper() == "APPROVED")
                .OrderByDescending(cr => cr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get user's submission history
        /// </summary>
        public async Task<List<CommunityRequest>> GetUserSubmissionsAsync(int userId)
        {
            return await _context.CommunityRequests
                .Include(cr => cr.Vacation)
                .Where(cr => cr.UserId == userId)
                .OrderByDescending(cr => cr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get active headlines (not expired)
        /// </summary>
        public async Task<List<Headline>> GetActiveHeadlinesAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Headlines
                .Where(h => h.ExpiresAt == null || h.ExpiresAt > now)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get status name by status_id
        /// </summary>
        public string GetStatusName(string status)
        {
            return status switch
            {
                "PENDING" => "Pending",
                "APPROVED" => "Approved",
                "REJECTED" => "Rejected",
                _ => "Unknown"
            };
        }
    }
}
