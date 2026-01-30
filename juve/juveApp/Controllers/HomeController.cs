using Microsoft.AspNetCore.Mvc;

namespace juveApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
