using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("users")]
    public class User
    {
        [Column("user_id")]
        [Key]
        public int UserId { get; set; }

        [Column("username")]
        [Required]
        [StringLength(255)]
        public string Username { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("bio")]
        public string? Bio { get; set; }

        [Column("role_id")]
        [Required]
        public int RoleId { get; set; } = 0; // Default to user role

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}
