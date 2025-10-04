using System.ComponentModel.DataAnnotations;

namespace HireHub.Models
{
    public class UserExperience
    {
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Company { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyWorking { get; set; } = false;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Achievements { get; set; }
    }
}
