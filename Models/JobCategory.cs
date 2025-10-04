using System.ComponentModel.DataAnnotations;

namespace HireHub.Models
{
    public class JobCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}
