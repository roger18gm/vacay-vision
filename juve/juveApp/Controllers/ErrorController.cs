using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace juveApp.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            ViewData["StatusCode"] = statusCode;

            return statusCode switch
            {
                404 => View("NotFound"),
                500 => View("ServerError"),
                _ => View("Error")
            };
        }

        [Route("500")]
        public IActionResult ServerError()
        {
            return View();
        }

        [Route("404")]
        public new IActionResult NotFound()
        {
            return View();
        }
    }
}
