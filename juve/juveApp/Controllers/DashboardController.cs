using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using juveApp.Services;

namespace juveApp.Controllers
{
    [Route("dashboard")]
    [Authorize(Roles = "2")] // Admin only (role_id = 2)
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            DashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// GET /dashboard - Main admin dashboard
        /// </summary>
        // [HttpGet("")]
        [HttpGet("/dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                var recentHeadlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                var pendingFeedback = await _dashboardService.GetPendingFeedbackAsync();

                ViewBag.Stats = stats;
                ViewBag.PendingRequests = pendingRequests;
                ViewBag.Headlines = recentHeadlines;
                ViewBag.PendingFeedback = pendingFeedback;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Failed to load dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET /dashboard/stats - Get current dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return PartialView("_DashboardStats", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return Content("<div id='dashboard-stats'></div>");
            }
        }

        /// <summary>
        /// POST /dashboard/approve/{id} - Approve a community request
        /// </summary>
        [HttpPost("approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            try
            {
                bool success = await _dashboardService.UpdateCommunityRequestStatusAsync(id, "approved");

                // For HTMX requests, return updated pending requests list and trigger stats update
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                    Response.Headers["HX-Trigger"] = success ? "updateStats" : "showError";
                    return PartialView("_PendingRequests", pendingRequests);
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Community request approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Request not found.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request");
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                    Response.Headers["HX-Trigger"] = "showError";
                    return PartialView("_PendingRequests", pendingRequests);
                }
                TempData["ErrorMessage"] = "Failed to approve request. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /dashboard/reject/{id} - Reject a community request
        /// </summary>
        [HttpPost("reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id, string? rejectionReason = null)
        {
            try
            {
                bool success = await _dashboardService.UpdateCommunityRequestStatusAsync(id, "rejected", rejectionReason);

                // For HTMX requests, return updated pending requests list and trigger stats update
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                    Response.Headers["HX-Trigger"] = success ? "updateStats" : "showError";
                    return PartialView("_PendingRequests", pendingRequests);
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Community request rejected.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Request not found.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request");
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                    Response.Headers["HX-Trigger"] = "showError";
                    return PartialView("_PendingRequests", pendingRequests);
                }
                TempData["ErrorMessage"] = "Failed to reject request. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /dashboard/search-user - Search for users
        /// </summary>
        [HttpPost("search-user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUser(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    if (Request.Headers["HX-Request"] == "true")
                    {
                        return Content("<div id='search-results' class='mt-4 text-gray-500'>Please enter a username to search.</div>");
                    }
                    TempData["ErrorMessage"] = "Please enter a username to search.";
                    return RedirectToAction("Index");
                }

                var users = await _dashboardService.SearchUsersByUsernameAsync(username);

                // For HTMX requests, return just the search results partial
                if (Request.Headers["HX-Request"] == "true")
                {
                    ViewBag.SearchQuery = username;
                    return PartialView("_UserSearchResults", users);
                }

                ViewBag.SearchResults = users;
                ViewBag.SearchQuery = username;

                var stats = await _dashboardService.GetDashboardStatsAsync();
                var pendingRequests = await _dashboardService.GetPendingRequestsAsync();
                var recentHeadlines = await _dashboardService.GetRecentHeadlinesAsync(5);

                ViewBag.Stats = stats;
                ViewBag.PendingRequests = pendingRequests;
                ViewBag.Headlines = recentHeadlines;

                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                if (Request.Headers["HX-Request"] == "true")
                {
                    return Content("<div id='search-results' class='mt-4 text-red-600'>Failed to search users. Please try again.</div>");
                }
                TempData["ErrorMessage"] = "Failed to search users. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /dashboard/create-headline - Create a new headline
        /// </summary>
        [HttpPost("create-headline")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHeadline(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    if (Request.Headers["HX-Request"] == "true")
                    {
                        Response.Headers["HX-Trigger"] = "showError";
                        var headlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                        return PartialView("_HeadlinesList", headlines);
                    }
                    TempData["ErrorMessage"] = "Headline content cannot be empty.";
                    return RedirectToAction("Index");
                }

                int adminUserId = GetCurrentUserId();
                await _dashboardService.CreateHeadlineAsync(content, adminUserId);

                // For HTMX requests, return updated headlines list
                if (Request.Headers["HX-Request"] == "true")
                {
                    var headlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                    Response.Headers["HX-Trigger"] = "headlineCreated";
                    return PartialView("_HeadlinesList", headlines);
                }

                TempData["SuccessMessage"] = "Headline posted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating headline");
                if (Request.Headers["HX-Request"] == "true")
                {
                    Response.Headers["HX-Trigger"] = "showError";
                    var headlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                    return PartialView("_HeadlinesList", headlines);
                }
                TempData["ErrorMessage"] = "Failed to create headline. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /dashboard/delete-headline/{id} - Delete a headline
        /// </summary>
        [HttpPost("delete-headline/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHeadline(int id)
        {
            try
            {
                bool success = await _dashboardService.DeleteHeadlineAsync(id);

                // For HTMX requests, return updated headlines list
                if (Request.Headers["HX-Request"] == "true")
                {
                    var headlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                    Response.Headers["HX-Trigger"] = success ? "headlineDeleted" : "showError";
                    return PartialView("_HeadlinesList", headlines);
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Headline deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Headline not found.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting headline");
                if (Request.Headers["HX-Request"] == "true")
                {
                    var headlines = await _dashboardService.GetRecentHeadlinesAsync(5);
                    Response.Headers["HX-Trigger"] = "showError";
                    return PartialView("_HeadlinesList", headlines);
                }
                TempData["ErrorMessage"] = "Failed to delete headline. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /dashboard/resolve-feedback/{id} - Mark feedback as resolved
        /// </summary>
        [HttpPost("resolve-feedback/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveFeedback(int id)
        {
            try
            {
                bool success = await _dashboardService.UpdateFeedbackStatusAsync(id, "resolved");

                // For HTMX requests, return updated pending feedback list
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingFeedback = await _dashboardService.GetPendingFeedbackAsync();
                    Response.Headers["HX-Trigger"] = success ? "feedbackResolved" : "showError";
                    return PartialView("_PendingFeedback", pendingFeedback);
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Feedback marked as resolved!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Feedback not found.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving feedback");
                if (Request.Headers["HX-Request"] == "true")
                {
                    var pendingFeedback = await _dashboardService.GetPendingFeedbackAsync();
                    Response.Headers["HX-Trigger"] = "showError";
                    return PartialView("_PendingFeedback", pendingFeedback);
                }
                TempData["ErrorMessage"] = "Failed to resolve feedback. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}
