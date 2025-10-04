using HireHub.Models;

namespace HireHub.Models
{
    public class AdminReportsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobSeekers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int PendingJobs { get; set; }
        public int ApprovedJobs { get; set; }
        public List<JobPosting> RecentJobs { get; set; } = new List<JobPosting>();
        public List<JobApplication> RecentApplications { get; set; } = new List<JobApplication>();
        public List<MonthlyData> JobPostingsByMonth { get; set; } = new List<MonthlyData>();
        public List<MonthlyData> ApplicationsByMonth { get; set; } = new List<MonthlyData>();
    }

    public class MonthlyData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}
