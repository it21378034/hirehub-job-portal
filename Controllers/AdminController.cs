using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HireHub.Data;
using HireHub.Models;
using HireHub.Services;

namespace HireHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobs = await _context.JobPostings.CountAsync(),
                TotalApplications = await _context.JobApplications.CountAsync(),
                ActiveJobs = await _context.JobPostings.CountAsync(j => j.IsActive),
                RecentJobs = await _context.JobPostings
                    .Include(j => j.Employer)
                    .OrderByDescending(j => j.PostedDate)
                    .Take(5)
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Include(ja => ja.JobPosting)
                    .Include(ja => ja.JobSeeker)
                    .OrderByDescending(ja => ja.AppliedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.JobPostings
                .Include(j => j.Employer)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            var job = await _context.JobPostings.FindAsync(id);
            if (job != null)
            {
                job.IsActive = !job.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job {(job.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction("Jobs");
        }

        public async Task<IActionResult> Applications()
        {
            var applications = await _context.JobApplications
                .Include(ja => ja.JobPosting)
                .Include(ja => ja.JobSeeker)
                .OrderByDescending(ja => ja.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, string status)
        {
            var application = await _context.JobApplications.FindAsync(id);
            if (application != null)
            {
                application.Status = status;
                application.ReviewedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Application status updated successfully!";
            }

            return RedirectToAction("Applications");
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var reports = new AdminReportsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobSeekers = await _context.Users.CountAsync(u => u.UserName != "admin@jobportal.com"),
                TotalEmployers = await _context.Users.CountAsync(u => u.UserName != "admin@jobportal.com"),
                TotalJobs = await _context.JobPostings.CountAsync(),
                TotalApplications = await _context.JobApplications.CountAsync(),
                PendingJobs = await _context.JobPostings.CountAsync(j => j.Status == "Pending"),
                ApprovedJobs = await _context.JobPostings.CountAsync(j => j.Status == "Approved"),
                RecentJobs = await _context.JobPostings
                    .Include(j => j.Employer)
                    .OrderByDescending(j => j.PostedDate)
                    .Take(10)
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Include(ja => ja.JobPosting)
                    .Include(ja => ja.JobSeeker)
                    .OrderByDescending(ja => ja.AppliedDate)
                    .Take(10)
                    .ToListAsync(),
                JobPostingsByMonth = await GetJobPostingsByMonth(),
                ApplicationsByMonth = await GetApplicationsByMonth()
            };

            return View(reports);
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.JobCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(JobCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.JobCategories.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var category = await _context.JobCategories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = !category.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category {(category.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJob(int id)
        {
            var job = await _context.JobPostings.FindAsync(id);
            if (job != null)
            {
                job.Status = "Approved";
                job.ApprovedAt = DateTime.UtcNow;
                job.ApprovedBy = User.Identity?.Name ?? "Admin";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job approved successfully!";
            }

            return RedirectToAction("Jobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectJob(int id)
        {
            var job = await _context.JobPostings.FindAsync(id);
            if (job != null)
            {
                job.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job rejected successfully!";
            }

            return RedirectToAction("Jobs");
        }

        private async Task<List<MonthlyData>> GetJobPostingsByMonth()
        {
            var data = await _context.JobPostings
                .Where(j => j.PostedDate >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(j => new { j.PostedDate.Year, j.PostedDate.Month })
                .Select(g => new MonthlyData
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToListAsync();

            return data;
        }

        private async Task<List<MonthlyData>> GetApplicationsByMonth()
        {
            var data = await _context.JobApplications
                .Where(ja => ja.AppliedDate >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(ja => new { ja.AppliedDate.Year, ja.AppliedDate.Month })
                .Select(g => new MonthlyData
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToListAsync();

            return data;
        }

        public async Task<IActionResult> EmailNotifications()
        {
            var applications = await _context.JobApplications
                .Include(ja => ja.JobPosting)
                .ThenInclude(jp => jp.Employer)
                .Include(ja => ja.JobSeeker)
                .OrderByDescending(ja => ja.AppliedDate)
                .ToListAsync();

            var emailStats = new
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == "Pending"),
                ShortlistedApplications = applications.Count(a => a.Status == "Shortlisted"),
                RejectedApplications = applications.Count(a => a.Status == "Rejected"),
                HiredApplications = applications.Count(a => a.Status == "Hired"),
                ApplicationsToday = applications.Count(a => a.AppliedDate.Date == DateTime.Today),
                ApplicationsThisWeek = applications.Count(a => a.AppliedDate >= DateTime.Today.AddDays(-7))
            };

            ViewBag.EmailStats = emailStats;
            return View(applications);
        }

        public IActionResult EmailSettings()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(string testEmail, string emailType)
        {
            try
            {
                switch (emailType)
                {
                    case "confirmation":
                        await _emailService.SendApplicationConfirmationAsync(
                            testEmail,
                            "Test User",
                            "Software Developer",
                            "Test Company"
                        );
                        break;
                    case "rejection":
                        await _emailService.SendApplicationRejectionAsync(
                            testEmail,
                            "Test User",
                            "Software Developer",
                            "Test Company"
                        );
                        break;
                    case "shortlist":
                        await _emailService.SendApplicationShortlistAsync(
                            testEmail,
                            "Test User",
                            "Software Developer",
                            "Test Company"
                        );
                        break;
                    case "notification":
                        await _emailService.SendNewApplicationNotificationAsync(
                            testEmail,
                            "Test Employer",
                            "Software Developer",
                            "Test Applicant"
                        );
                        break;
                }

                TempData["SuccessMessage"] = $"Test {emailType} email sent successfully to {testEmail}!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to send test email: {ex.Message}";
            }

            return RedirectToAction("EmailSettings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBulkEmail(string[] applicationIds, string emailType, string customMessage)
        {
            try
            {
                var applications = await _context.JobApplications
                    .Where(ja => applicationIds.Contains(ja.Id.ToString()))
                    .Include(ja => ja.JobPosting)
                    .Include(ja => ja.JobSeeker)
                    .ToListAsync();

                int successCount = 0;
                foreach (var application in applications)
                {
                    try
                    {
                        switch (emailType)
                        {
                            case "rejection":
                                await _emailService.SendApplicationRejectionAsync(
                                    application.JobSeeker.Email!,
                                    $"{application.JobSeeker.FirstName} {application.JobSeeker.LastName}",
                                    application.JobPosting.Title,
                                    application.JobPosting.Company
                                );
                                application.Status = "Rejected";
                                break;
                            case "shortlist":
                                await _emailService.SendApplicationShortlistAsync(
                                    application.JobSeeker.Email!,
                                    $"{application.JobSeeker.FirstName} {application.JobSeeker.LastName}",
                                    application.JobPosting.Title,
                                    application.JobPosting.Company
                                );
                                application.Status = "Shortlisted";
                                break;
                        }
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log individual failures but continue
                        Console.WriteLine($"Failed to send email for application {application.Id}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bulk email sent successfully to {successCount} out of {applications.Count} applications!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to send bulk emails: {ex.Message}";
            }

            return RedirectToAction("EmailNotifications");
        }
    }
}
