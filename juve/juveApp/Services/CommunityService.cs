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
                .Where(cr => cr.Status.ToLower() == "approved" && cr.Vacation != null)
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

        /// <summary>
        /// Get user's vacations that haven't been submitted to community
        /// </summary>
        public async Task<List<Vacation>> GetUserVacationsNotSubmittedAsync(int userId)
        {
            // Get all vacation IDs that have been submitted
            var submittedVacationIds = await _context.CommunityRequests
                .Where(cr => cr.UserId == userId)
                .Select(cr => cr.VacationId)
                .ToListAsync();

            // Get vacations not in the submitted list
            return await _context.Vacations
                .Where(v => v.UserId == userId && !submittedVacationIds.Contains(v.VacationId))
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Submit a vacation for community approval
        /// </summary>
        public async Task<CommunityRequest> SubmitVacationAsync(int vacationId, int userId)
        {
            // Check if vacation exists and belongs to user
            var vacation = await _context.Vacations
                .FirstOrDefaultAsync(v => v.VacationId == vacationId && v.UserId == userId);

            if (vacation == null)
                throw new InvalidOperationException("Vacation not found or does not belong to user");

            // Check if already submitted
            var existingRequest = await _context.CommunityRequests
                .FirstOrDefaultAsync(cr => cr.VacationId == vacationId && cr.UserId == userId);

            if (existingRequest != null)
                throw new InvalidOperationException("This vacation has already been submitted");

            var request = new CommunityRequest
            {
                VacationId = vacationId,
                UserId = userId,
                Status = "pending",
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CommunityRequests.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }
    }
}
