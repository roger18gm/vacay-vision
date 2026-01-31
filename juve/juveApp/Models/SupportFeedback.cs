using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("support_feedback")]
    public class SupportFeedback
    {
        [Column("support_id")]
        [Key]
        public int SupportId { get; set; }

        [Column("submitted_by")]
        [Required]
        public int SubmittedById { get; set; }

        [Column("type")]
        [Required]
        [StringLength(50)]
        public string FeedbackType { get; set; } = "general";

        [Column("message")]
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        [ForeignKey("SubmittedById")]
        public User? User { get; set; }
    }
}
