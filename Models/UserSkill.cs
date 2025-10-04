using System.ComponentModel.DataAnnotations;

namespace HireHub.Models
{
    public class UserSkill
    {
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ProficiencyLevel { get; set; } // Beginner, Intermediate, Advanced, Expert

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
