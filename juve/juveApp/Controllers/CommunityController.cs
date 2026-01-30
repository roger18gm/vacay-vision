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
    }
}
