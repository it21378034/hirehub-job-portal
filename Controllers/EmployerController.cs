using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HireHub.Data;
using HireHub.Models;
using HireHub.Services;

namespace HireHub.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public EmployerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Applications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var applications = await _context.JobApplications
                .Where(ja => ja.JobPosting.EmployerId == user.Id)
                .Include(ja => ja.JobPosting)
                .Include(ja => ja.JobSeeker)
                .OrderByDescending(ja => ja.AppliedDate)
                .ToListAsync();

            // Debug logging
            Console.WriteLine($"=== EMPLOYER APPLICATIONS DEBUG ===");
            Console.WriteLine($"Employer ID: {user.Id}");
            Console.WriteLine($"Total Applications Found: {applications.Count}");
            foreach (var app in applications)
            {
                Console.WriteLine($"App ID: {app.Id}, Cover Letter: {app.CoverLetter?.Substring(0, Math.Min(50, app.CoverLetter?.Length ?? 0))}..., Notes: {app.Notes?.Substring(0, Math.Min(50, app.Notes?.Length ?? 0))}..., Resume Path: {app.ResumePath}");
            }
            Console.WriteLine($"=====================================");

            return View(applications);
        }

        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var application = await _context.JobApplications
                .Where(ja => ja.Id == id && ja.JobPosting.EmployerId == user.Id)
                .Include(ja => ja.JobPosting)
                .Include(ja => ja.JobSeeker)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound();
            }

            // Debug logging for resume path
            Console.WriteLine($"=== APPLICATION DETAILS DEBUG ===");
            Console.WriteLine($"Application ID: {application.Id}");
            Console.WriteLine($"Resume Path: {application.ResumePath ?? "NULL"}");
            Console.WriteLine($"Cover Letter: {application.CoverLetter?.Substring(0, Math.Min(50, application.CoverLetter?.Length ?? 0))}...");
            Console.WriteLine($"Notes: {application.Notes?.Substring(0, Math.Min(50, application.Notes?.Length ?? 0))}...");
            Console.WriteLine($"Status: {application.Status}");
            Console.WriteLine($"Applied Date: {application.AppliedDate}");
            Console.WriteLine($"Job Seeker: {application.JobSeeker.FirstName} {application.JobSeeker.LastName}");
            Console.WriteLine($"Job Posting: {application.JobPosting.Title}");
            Console.WriteLine($"=====================================");

            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, string status, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var application = await _context.JobApplications
                .Where(ja => ja.Id == id && ja.JobPosting.EmployerId == user.Id)
                .Include(ja => ja.JobPosting)
                .Include(ja => ja.JobSeeker)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound();
            }

            var oldStatus = application.Status;
            application.Status = status;
            application.Notes = notes;
            application.ReviewedDate = DateTime.UtcNow;

            _context.Update(application);
            await _context.SaveChangesAsync();

            // Send appropriate email based on status change
            try
            {
                if (status == "Rejected" && oldStatus != "Rejected")
                {
                    await _emailService.SendApplicationRejectionAsync(
                        application.JobSeeker.Email!,
                        $"{application.JobSeeker.FirstName} {application.JobSeeker.LastName}",
                        application.JobPosting.Title,
                        application.JobPosting.Company
                    );
                }
                else if (status == "Shortlisted" && oldStatus != "Shortlisted")
                {
                    await _emailService.SendApplicationShortlistAsync(
                        application.JobSeeker.Email!,
                        $"{application.JobSeeker.FirstName} {application.JobSeeker.LastName}",
                        application.JobPosting.Title,
                        application.JobPosting.Company
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the status update
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }

            TempData["SuccessMessage"] = $"Application status updated to {status} successfully!";
            return RedirectToAction("ApplicationDetails", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ViewResume(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var application = await _context.JobApplications
                .Where(ja => ja.Id == id && ja.JobPosting.EmployerId == user.Id)
                .Include(ja => ja.JobSeeker)
                .FirstOrDefaultAsync();

            if (application == null || string.IsNullOrEmpty(application.ResumePath))
            {
                TempData["ErrorMessage"] = "Resume not found or not available.";
                return RedirectToAction("ApplicationDetails", new { id });
            }

            // Remove leading slash from ResumePath for file system access
            var resumePath = application.ResumePath.StartsWith("/") ? application.ResumePath.Substring(1) : application.ResumePath;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resumePath);
            
            Console.WriteLine($"=== RESUME FILE DEBUG ===");
            Console.WriteLine($"ResumePath from DB: {application.ResumePath}");
            Console.WriteLine($"ResumePath for file system: {resumePath}");
            Console.WriteLine($"Full file path: {filePath}");
            Console.WriteLine($"File exists: {System.IO.File.Exists(filePath)}");
            Console.WriteLine($"Directory exists: {Directory.Exists(Path.GetDirectoryName(filePath))}");
            Console.WriteLine($"=========================");
            
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Resume file not found on server.";
                return RedirectToAction("ApplicationDetails", new { id });
            }

            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = $"Resume_{application.JobSeeker.FirstName}_{application.JobSeeker.LastName}_{application.Id}.pdf";
                
                // Return file for inline viewing
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing resume: {ex.Message}");
                TempData["ErrorMessage"] = "Error viewing resume file.";
                return RedirectToAction("ApplicationDetails", new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadResume(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var application = await _context.JobApplications
                .Where(ja => ja.Id == id && ja.JobPosting.EmployerId == user.Id)
                .Include(ja => ja.JobSeeker)
                .FirstOrDefaultAsync();

            if (application == null || string.IsNullOrEmpty(application.ResumePath))
            {
                TempData["ErrorMessage"] = "Resume not found or not available.";
                return RedirectToAction("ApplicationDetails", new { id });
            }

            // Remove leading slash from ResumePath for file system access
            var resumePath = application.ResumePath.StartsWith("/") ? application.ResumePath.Substring(1) : application.ResumePath;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resumePath);
            
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Resume file not found on server.";
                return RedirectToAction("ApplicationDetails", new { id });
            }

            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = $"Resume_{application.JobSeeker.FirstName}_{application.JobSeeker.LastName}_{application.Id}.pdf";
                
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading resume: {ex.Message}");
                TempData["ErrorMessage"] = "Error downloading resume file.";
                return RedirectToAction("ApplicationDetails", new { id });
            }
        }
    }
}
