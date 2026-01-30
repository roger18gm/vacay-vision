using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace juveApp.Models
{
    [Table("vacations")]
    public class Vacation
    {
        [Column("vacation_id")]
        [Key]
        public int VacationId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("title")]
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("destination")]
        [Required]
        [StringLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<CommunityRequest> CommunityRequests { get; set; } = new List<CommunityRequest>();
        public ICollection<VacationComment> Comments { get; set; } = new List<VacationComment>();
        public ICollection<VacationWidget> Widgets { get; set; } = new List<VacationWidget>();
    }
}
