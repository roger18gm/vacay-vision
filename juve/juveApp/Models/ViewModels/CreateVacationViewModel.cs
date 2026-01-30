using System.ComponentModel.DataAnnotations;

namespace juveApp.Models.ViewModels
{
    public class CreateVacationViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(255, ErrorMessage = "Destination cannot exceed 255 characters")]
        [Display(Name = "Destination")]
        public string Destination { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Display(Name = "Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ImageUrl { get; set; }
    }
}
