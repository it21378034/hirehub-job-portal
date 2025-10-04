using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        public string? CurrentPosition { get; set; }

        [StringLength(100)]
        public string? CurrentCompany { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(200)]
        public string? LinkedInProfile { get; set; }

        [StringLength(200)]
        public string? GitHubProfile { get; set; }

        [StringLength(500)]
        public string? ResumePath { get; set; }

        [StringLength(50)]
        public string? ResumeFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
        public virtual ICollection<UserEducation> Education { get; set; } = new List<UserEducation>();
        public virtual ICollection<UserExperience> Experience { get; set; } = new List<UserExperience>();
    }
}
