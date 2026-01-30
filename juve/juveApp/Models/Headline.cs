using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("horizon_headlines")]
    public class Headline
    {
        [Column("headline_id")]
        [Key]
        public int HeadlineId { get; set; }

        [Column("message")]
        [Required]
        public string Message { get; set; } = string.Empty;

        [Column("created_by")]
        [Required]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        // Navigation property
        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }
    }
}
