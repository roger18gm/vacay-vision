using Microsoft.AspNetCore.Mvc;
using juveApp.Services;
using juveApp.Models;

namespace juveApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly CommunityService _communityService;

        public HomeController(CommunityService communityService)
        {
            _communityService = communityService;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get recent approved vacations for home page preview
                var recentVacations = (await _communityService.GetApprovedVacationsAsync())
                    .Take(3)
                    .ToList();

                ViewBag.RecentVacations = recentVacations;
                return View();
            }
            catch
            {
                // If database fails, still show home page
                ViewBag.RecentVacations = new List<CommunityRequest>();
                return View();
            }
        }
    }
}
