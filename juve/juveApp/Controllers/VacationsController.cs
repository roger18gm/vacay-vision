using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using juveApp.Services;
using juveApp.Models.ViewModels;

namespace juveApp.Controllers
{
    [Route("vacations")]
    [Authorize] // All actions require authentication
    public class VacationsController : Controller
    {
        private readonly VacationService _vacationService;
        private readonly CommentService _commentService;
        private readonly ILogger<VacationsController> _logger;

        public VacationsController(
            VacationService vacationService,
            CommentService commentService,
            ILogger<VacationsController> logger)
        {
            _vacationService = vacationService;
            _commentService = commentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// GET /vacations - List user's vacations
        /// </summary>
        [HttpGet("/vacations")]
        public async Task<IActionResult> Index()
        {
            try
            {
                int userId = GetCurrentUserId();
                var vacations = await _vacationService.GetUserVacationsAsync(userId);
                return View(vacations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vacations");
                TempData["ErrorMessage"] = "Failed to load vacations. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET /vacations/create - Show create form
        /// </summary>
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new CreateVacationViewModel());
        }

        /// <summary>
        /// POST /vacations/create - Process create form
        /// </summary>
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVacationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int userId = GetCurrentUserId();
                var vacation = await _vacationService.CreateVacationAsync(model, userId);
                TempData["SuccessMessage"] = "Vacation created successfully!";
                return RedirectToAction("Detail", new { id = vacation.VacationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vacation");
                TempData["ErrorMessage"] = "Failed to create vacation. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// GET /vacations/{id} - Show vacation details with comments
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var vacation = await _vacationService.GetVacationByIdAsync(id);

                if (vacation == null)
                {
                    TempData["ErrorMessage"] = "Vacation not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.CurrentUserId = GetCurrentUserId();
                return View(vacation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vacation details");
                TempData["ErrorMessage"] = "Failed to load vacation details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// GET /vacations/{id}/edit - Show edit form (verify ownership)
        /// </summary>
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                if (!await _vacationService.UserOwnsVacationAsync(id, userId))
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this vacation.";
                    return RedirectToAction("Index");
                }

                var vacation = await _vacationService.GetVacationByIdAsync(id);

                if (vacation == null)
                {
                    TempData["ErrorMessage"] = "Vacation not found.";
                    return RedirectToAction("Index");
                }

                var model = new EditVacationViewModel
                {
                    VacationId = vacation.VacationId,
                    Title = vacation.Title,
                    Destination = vacation.Destination,
                    Description = vacation.Description,
                    ImageUrl = vacation.ImageUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vacation for edit");
                TempData["ErrorMessage"] = "Failed to load vacation. Please try again.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /vacations/{id} - Process edit form
        /// </summary>
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, EditVacationViewModel model)
        {
            if (id != model.VacationId)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            try
            {
                int userId = GetCurrentUserId();
                bool success = await _vacationService.UpdateVacationAsync(model, userId);

                if (!success)
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this vacation.";
                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = "Vacation updated successfully!";
                return RedirectToAction("Detail", new { id = model.VacationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vacation");
                TempData["ErrorMessage"] = "Failed to update vacation. Please try again.";
                return View("Edit", model);
            }
        }

        /// <summary>
        /// POST /vacations/{id}/delete - Delete vacation (verify ownership)
        /// </summary>
        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool success = await _vacationService.DeleteVacationAsync(id, userId);

                if (!success)
                {
                    TempData["ErrorMessage"] = "You don't have permission to delete this vacation.";
                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = "Vacation deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vacation");
                TempData["ErrorMessage"] = "Failed to delete vacation. Please try again.";
                return RedirectToAction("Detail", new { id });
            }
        }

        /// <summary>
        /// POST /vacations/{id}/comments - Add comment
        /// </summary>
        [HttpPost("{id}/comments")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, AddCommentViewModel model)
        {
            if (id != model.VacationId)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Detail", new { id });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("Detail", new { id });
            }

            try
            {
                int userId = GetCurrentUserId();
                await _commentService.AddCommentAsync(model, userId);
                TempData["SuccessMessage"] = "Comment added successfully!";
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                TempData["ErrorMessage"] = "Failed to add comment. Please try again.";
                return RedirectToAction("Detail", new { id });
            }
        }

        /// <summary>
        /// POST /vacations/{id}/comments/{commentId}/delete - Delete comment
        /// </summary>
        [HttpPost("{id}/comments/{commentId}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int commentId)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool success = await _commentService.DeleteCommentAsync(commentId, userId);

                if (!success)
                {
                    TempData["ErrorMessage"] = "You don't have permission to delete this comment.";
                    return RedirectToAction("Detail", new { id });
                }

                TempData["SuccessMessage"] = "Comment deleted successfully!";
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                TempData["ErrorMessage"] = "Failed to delete comment. Please try again.";
                return RedirectToAction("Detail", new { id });
            }
        }
    }
}
