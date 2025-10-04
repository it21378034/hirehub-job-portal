using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class JobPosting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Company { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string JobType { get; set; } = string.Empty; // Full-time, Part-time, Contract, etc.

        [Required]
        [StringLength(100)]
        public string ExperienceLevel { get; set; } = string.Empty; // Entry, Mid, Senior, etc.

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalaryMax { get; set; }

        [StringLength(50)]
        public string? SalaryCurrency { get; set; }

        [Required]
        public string Requirements { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Benefits { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApplicationDeadline { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        public string EmployerId { get; set; } = string.Empty;
        public virtual ApplicationUser? Employer { get; set; }

        public int? CategoryId { get; set; }
        public virtual JobCategory? Category { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Closed

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
