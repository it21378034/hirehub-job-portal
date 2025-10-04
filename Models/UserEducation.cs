using System.ComponentModel.DataAnnotations;

namespace HireHub.Models
{
    public class UserEducation
    {
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Institution { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Degree { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FieldOfStudy { get; set; }

        [StringLength(10)]
        public string? Grade { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyStudying { get; set; } = false;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
