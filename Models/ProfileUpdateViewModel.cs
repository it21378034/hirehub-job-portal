using System.ComponentModel.DataAnnotations;

namespace HireHub.Models
{
    public class ProfileUpdateViewModel
    {
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
    }
}
