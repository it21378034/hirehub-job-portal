using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireHub.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public int JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; } = null!;

        [Required]
        public string JobSeekerId { get; set; } = string.Empty;
        public virtual ApplicationUser JobSeeker { get; set; } = null!;

        [StringLength(500)]
        public string? CoverLetter { get; set; }

        [StringLength(500)]
        public string? ResumePath { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Shortlisted, Rejected, Hired

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ReviewedDate { get; set; }
    }
}
