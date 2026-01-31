using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using juveApp.Services;

namespace juveApp.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }

        /// <summary>
        /// GET /users/{id} - View user profile
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Profile(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Home");
                }

                var vacations = await _userService.GetUserVacationsAsync(id);
                var stats = await _userService.GetUserStatsAsync(id);

                ViewBag.Vacations = vacations;
                ViewBag.Stats = stats;
                ViewBag.CurrentUserId = GetCurrentUserId();
                ViewBag.IsOwnProfile = GetCurrentUserId() == id;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["ErrorMessage"] = "Failed to load user profile.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// POST /users/{id}/feedback - Submit support feedback
        /// </summary>
        [HttpPost("{id}/feedback")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int id, string feedbackType, string message)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to submit feedback.";
                    return RedirectToAction("Login", "Auth");
                }

                // Can only submit feedback on own profile
                if (currentUserId != id)
                {
                    TempData["ErrorMessage"] = "You can only submit feedback on your own profile.";
                    return RedirectToAction("Profile", new { id });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    TempData["ErrorMessage"] = "Feedback message cannot be empty.";
                    return RedirectToAction("Profile", new { id });
                }

                await _userService.CreateFeedbackAsync(currentUserId.Value, feedbackType ?? "general", message);

                TempData["SuccessMessage"] = "Thank you for your feedback! We'll review it shortly.";
                return RedirectToAction("Profile", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                TempData["ErrorMessage"] = "Failed to submit feedback. Please try again.";
                return RedirectToAction("Profile", new { id });
            }
        }
    }
}
