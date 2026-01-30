using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("vacation_comments")]
    public class VacationComment
    {
        [Column("comment_id")]
        [Key]
        public int CommentId { get; set; }

        [Column("vacation_id")]
        [Required]
        public int VacationId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("content")]
        [Required]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VacationId")]
        public Vacation? Vacation { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
