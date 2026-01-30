using System.ComponentModel.DataAnnotations;

namespace juveApp.Models.ViewModels
{
    public class AddCommentViewModel
    {
        [Required]
        public int VacationId { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 2000 characters")]
        [Display(Name = "Comment")]
        public string Content { get; set; } = string.Empty;
    }
}
