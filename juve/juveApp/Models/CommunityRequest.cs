using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("community_requests")]
    public class CommunityRequest
    {
        [Column("request_id")]
        [Key]
        public int RequestId { get; set; }

        [Column("vacation_id")]
        [Required]
        public int VacationId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; }

        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VacationId")]
        public Vacation? Vacation { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
