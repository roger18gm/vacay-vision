using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using juveApp.Models.ViewModels;
using juveApp.Services;

namespace juveApp.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// GET /auth/login - Show login form
        /// </summary>
        [HttpGet("login")]
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/");

            return View();
        }

        /// <summary>
        /// POST /auth/login - Process login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.AuthenticateUserAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Store claims in authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // Set login time in session
            HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString());

            TempData["SuccessMessage"] = "Welcome back! You have successfully logged in.";
            return Redirect("/");
        }

        /// <summary>
        /// GET /auth/signup - Show signup form
        /// </summary>
        [HttpGet("signup")]
        public IActionResult Signup()
        {
            // If already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/");

            return View();
        }

        /// <summary>
        /// POST /auth/signup - Create new user account
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate custom rules
            var errors = await _authService.ValidateSignupAsync(model);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    ModelState.AddModelError("", error);
                return View(model);
            }

            var newUser = await _authService.CreateUserAsync(model);

            TempData["SuccessMessage"] = "Account created successfully! Please log in with your new credentials.";
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// POST /auth/logout - Sign out user
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Destroy session
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "Goodbye! You have been successfully logged out.";
            return Redirect("/");
        }
    }
}
