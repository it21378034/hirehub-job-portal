using HireHub.Models;

namespace HireHub.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int ActiveJobs { get; set; }
        public List<JobPosting> RecentJobs { get; set; } = new List<JobPosting>();
        public List<JobApplication> RecentApplications { get; set; } = new List<JobApplication>();
    }
}
