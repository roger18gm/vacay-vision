using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using juveApp.Services;
using System.Security.Claims;

namespace juveApp.Controllers
{
    [Route("community")]
    [Authorize] // Require login for all community routes
    public class CommunityController : Controller
    {
        private readonly CommunityService _communityService;

        public CommunityController(CommunityService communityService)
        {
            _communityService = communityService;
        }

        /// <summary>
        /// GET /community - Show approved vacation submissions
        /// </summary>
        [HttpGet("/community")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var approvedVacations = await _communityService.GetApprovedVacationsAsync();
                var headlines = await _communityService.GetActiveHeadlinesAsync();

                ViewBag.Headlines = headlines;
                return View(approvedVacations);
            }
            catch (Exception ex)
            {
                // Log error
                TempData["ErrorMessage"] = "Failed to load community page";
                return Redirect("/");
            }
        }

        /// <summary>
        /// GET /community/my-submissions - View user's submission history
        /// </summary>
        [HttpGet("my-submissions")]
        public async Task<IActionResult> MySubmissions()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Redirect("/auth/login");
                }

                int userId = int.Parse(userIdClaim.Value);
                var submissions = await _communityService.GetUserSubmissionsAsync(userId);

                return View(submissions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load submissions";
                return Redirect("/community");
            }
        }

        /// <summary>
        /// GET /community/submit - Show vacation submission form
        /// </summary>
        [HttpGet("submit")]
        public async Task<IActionResult> Submit()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Redirect("/auth/login");
                }

                int userId = int.Parse(userIdClaim.Value);
                var availableVacations = await _communityService.GetUserVacationsNotSubmittedAsync(userId);

                if (availableVacations.Count == 0)
                {
                    TempData["ErrorMessage"] = "You don't have any vacations to submit. Create a vacation first!";
                    return RedirectToAction("Index", "Vacations");
                }

                return View(availableVacations);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load submission form";
                return Redirect("/community");
            }
        }

        /// <summary>
        /// POST /community/submit - Submit a vacation for approval
        /// </summary>
        [HttpPost("submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitVacation(int vacationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Redirect("/auth/login");
                }

                int userId = int.Parse(userIdClaim.Value);
                await _communityService.SubmitVacationAsync(vacationId, userId);

                TempData["SuccessMessage"] = "Vacation submitted for approval! You'll be notified once it's reviewed.";
                return RedirectToAction("MySubmissions");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Submit");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to submit vacation. Please try again.";
                return RedirectToAction("Submit");
            }
        }
    }
}
