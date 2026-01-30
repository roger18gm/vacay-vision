using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("vacation_widgets")]
    public class VacationWidget
    {
        [Column("vacation_widget_id")]
        [Key]
        public int VacationWidgetId { get; set; }

        [Column("vacation_id")]
        [Required]
        public int VacationId { get; set; }

        [Column("title")]
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("type")]
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [Column("content")]
        [Required]
        public string Content { get; set; } = string.Empty;

        [Column("external_url")]
        public string? ExternalUrl { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VacationId")]
        public Vacation? Vacation { get; set; }
    }
}
