using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HireHub.Data;
using HireHub.Models;
using HireHub.Services;

namespace HireHub.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            // Force a complete context refresh to ensure we get the latest data
            _context.ChangeTracker.Clear();
            
            var jobs = await _context.JobPostings
                .Where(j => j.IsActive && j.Status == "Approved")
                .Include(j => j.Employer)
                .Include(j => j.Category)
                .AsNoTracking()
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Force a complete context refresh to ensure we get the latest data
            _context.ChangeTracker.Clear();
            
            var job = await _context.JobPostings
                .Include(j => j.Employer)
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            // Debug logging for Details action
            Console.WriteLine($"=== DETAILS DEBUG ===");
            Console.WriteLine($"Job ID: {job.Id}");
            Console.WriteLine($"Title: {job.Title}");
            Console.WriteLine($"Company: {job.Company}");
            Console.WriteLine($"Description: {job.Description?.Substring(0, Math.Min(50, job.Description?.Length ?? 0))}...");
            Console.WriteLine($"Last Modified: {job.PostedDate}");
            Console.WriteLine($"=====================");

            return View(job);
        }

        [HttpGet]
        [Authorize(Roles = "Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPosting model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }

            // Set required fields before validation
            model.EmployerId = user.Id;
            model.PostedDate = DateTime.UtcNow;
            model.IsActive = true;
            model.Status = "Pending"; // New jobs require admin approval

            // Remove validation errors for fields we're setting manually
            ModelState.Remove("EmployerId");
            ModelState.Remove("Employer");
            ModelState.Remove("PostedDate");
            ModelState.Remove("IsActive");
            ModelState.Remove("Status");

            // Debug: Log model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);
                Console.WriteLine($"Validation errors: {errorMessage}");
                TempData["ErrorMessage"] = $"Please correct the errors: {errorMessage}";
                return View(model);
            }

            try
            {
                _context.JobPostings.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Job posted successfully! It will be reviewed by admin before going live.";
                return RedirectToAction("MyJobs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating job: {ex.Message}");
                TempData["ErrorMessage"] = $"Error creating job: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> MyJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Force a complete context refresh to ensure we get the latest data
            _context.ChangeTracker.Clear();

            var jobs = await _context.JobPostings
                .Where(j => j.EmployerId == user.Id)
                .Include(j => j.Applications)
                .AsNoTracking()
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            // Debug logging for MyJobs
            Console.WriteLine($"=== MY JOBS DEBUG ===");
            Console.WriteLine($"User ID: {user.Id}");
            Console.WriteLine($"Total Jobs Found: {jobs.Count}");
            foreach (var job in jobs)
            {
                Console.WriteLine($"Job ID: {job.Id}, Title: {job.Title}, Company: {job.Company}, Posted: {job.PostedDate}");
            }
            Console.WriteLine($"=====================");

            return View(jobs);
        }

        [HttpGet]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var job = await _context.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == user!.Id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobPosting model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Verify the job belongs to the current user
            var existingJob = await _context.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == user.Id);

            if (existingJob == null)
            {
                return NotFound();
            }

            // Remove Employer validation errors since we're updating an existing job
            ModelState.Remove("Employer");
            ModelState.Remove("Applications");
            ModelState.Remove("Category");

            // Debug validation errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== VALIDATION ERRORS ===");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Field: {error.Key}");
                        foreach (var err in error.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {err.ErrorMessage}");
                        }
                    }
                }
                Console.WriteLine("========================");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Enhanced debugging and database check
                    Console.WriteLine($"=== JOB UPDATE DEBUG ===");
                    Console.WriteLine($"Job ID: {existingJob.Id}");
                    Console.WriteLine($"Old Title: {existingJob.Title}");
                    Console.WriteLine($"New Title: {model.Title}");
                    Console.WriteLine($"Old Company: {existingJob.Company}");
                    Console.WriteLine($"New Company: {model.Company}");
                    Console.WriteLine($"Database Connection State: {_context.Database.GetDbConnection().State}");
                    
                    // Check database file
                    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "jobportal.db");
                    Console.WriteLine($"Database file path: {dbPath}");
                    Console.WriteLine($"Database file exists: {System.IO.File.Exists(dbPath)}");
                    Console.WriteLine($"=========================");

                    // Update the existing job with the new values
                    existingJob.Title = model.Title ?? "";
                    existingJob.Company = model.Company ?? "";
                    existingJob.Location = model.Location ?? "";
                    existingJob.Description = model.Description ?? "";
                    existingJob.JobType = model.JobType ?? "";
                    existingJob.ExperienceLevel = model.ExperienceLevel ?? "";
                    existingJob.ApplicationDeadline = model.ApplicationDeadline;
                    existingJob.SalaryMin = model.SalaryMin;
                    existingJob.SalaryMax = model.SalaryMax;
                    existingJob.SalaryCurrency = model.SalaryCurrency ?? "";
                    existingJob.Requirements = model.Requirements ?? "";
                    existingJob.Benefits = model.Benefits ?? "";

                    // Save changes using Entity Framework
                    var result = await _context.SaveChangesAsync();
                    Console.WriteLine($"EF Core update result: {result} rows affected");

                    // Clear the context to ensure fresh data on next read
                    _context.ChangeTracker.Clear();

                    TempData["SuccessMessage"] = $"Job posting '{model.Title}' updated successfully!";
                    return RedirectToAction("Details", new { id = existingJob.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobPostingExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating job: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Don't show error messages to user, just log them
                    return View(model);
                }
            }

            // If model state is invalid, return to view with errors
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var job = await _context.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == user!.Id);

            if (job != null)
            {
                _context.JobPostings.Remove(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job posting deleted successfully!";
            }

            return RedirectToAction("MyJobs");
        }

        [HttpGet]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.JobPostings
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => ja.JobPostingId == id && ja.JobSeekerId == user!.Id);

            if (existingApplication != null)
            {
                TempData["InfoMessage"] = "You have already applied for this job.";
                return RedirectToAction("Details", new { id });
            }

            ViewBag.Job = job;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, JobApplication model, IFormFile? resumeFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Remove validation errors for properties that we set manually
            ModelState.Remove("JobSeeker");
            ModelState.Remove("JobPosting");
            ModelState.Remove("JobSeekerId");
            ModelState.Remove("resumeFile");
            
            // Clear any validation errors for resumeFile specifically
            if (ModelState.ContainsKey("resumeFile"))
            {
                ModelState["resumeFile"].Errors.Clear();
            }
            
            Console.WriteLine($"ModelState.IsValid after removing all manual properties: {ModelState.IsValid}");
            Console.WriteLine($"Cover Letter: {model.CoverLetter}");
            Console.WriteLine($"Notes: {model.Notes}");
            Console.WriteLine($"Resume File: {(resumeFile != null ? resumeFile.FileName : "None")}");
            Console.WriteLine($"Model State Keys: {string.Join(", ", ModelState.Keys)}");
            
            // Check if model properties are being set correctly
            Console.WriteLine($"=== MODEL BINDING DEBUG ===");
            Console.WriteLine($"Model.CoverLetter: '{model.CoverLetter}'");
            Console.WriteLine($"Model.Notes: '{model.Notes}'");
            Console.WriteLine($"Model.JobPostingId: {model.JobPostingId}");
            Console.WriteLine($"Model.JobSeekerId: '{model.JobSeekerId}'");
            Console.WriteLine($"Model.Status: '{model.Status}'");
            Console.WriteLine($"Model.AppliedDate: {model.AppliedDate}");
            Console.WriteLine($"=============================");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);
                Console.WriteLine($"Validation errors after removing manual properties: {errorMessage}");
                
                // Log each individual error
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        Console.WriteLine($"Key: {key}, Errors: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                TempData["ErrorMessage"] = $"Please correct the errors: {errorMessage}";
                return RedirectToAction("Apply", new { id });
            }

            if (ModelState.IsValid)
            {
                // Check if user has already applied for this job
                var existingApplication = await _context.JobApplications
                    .FirstOrDefaultAsync(ja => ja.JobPostingId == id && ja.JobSeekerId == user.Id);

                if (existingApplication != null)
                {
                    TempData["ErrorMessage"] = "You have already applied for this job position.";
                    return RedirectToAction("Details", new { id });
                }

                model.JobPostingId = id;
                model.JobSeekerId = user.Id;
                model.AppliedDate = DateTime.UtcNow;

                // Handle resume file upload
                if (resumeFile != null && resumeFile.Length > 0)
                {
                    try
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var fileExtension = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["ErrorMessage"] = "Please upload a valid resume file (PDF, DOC, or DOCX).";
                            return RedirectToAction("Apply", new { id });
                        }

                        // Validate file size (10MB max)
                        if (resumeFile.Length > 10 * 1024 * 1024)
                        {
                            TempData["ErrorMessage"] = "Resume file size must be less than 10MB.";
                            return RedirectToAction("Apply", new { id });
                        }

                        // Create uploads directory if it doesn't exist
                        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
                        Directory.CreateDirectory(uploadsDir);

                        // Generate unique filename
                        var fileName = $"resume_{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                        var filePath = Path.Combine(uploadsDir, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await resumeFile.CopyToAsync(stream);
                        }

                        // Set the resume path (relative to wwwroot)
                        model.ResumePath = $"/uploads/resumes/{fileName}";
                        
                        Console.WriteLine($"Resume uploaded successfully: {model.ResumePath}");
                        Console.WriteLine($"Full file path: {filePath}");
                        Console.WriteLine($"File exists: {System.IO.File.Exists(filePath)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading resume: {ex.Message}");
                        TempData["ErrorMessage"] = "Error uploading resume file. Please try again.";
                        return RedirectToAction("Apply", new { id });
                    }
                }

                // Debug logging before saving
                Console.WriteLine($"=== BEFORE SAVING APPLICATION ===");
                Console.WriteLine($"JobPostingId: {model.JobPostingId}");
                Console.WriteLine($"JobSeekerId: {model.JobSeekerId}");
                Console.WriteLine($"CoverLetter: {model.CoverLetter}");
                Console.WriteLine($"ResumePath: {model.ResumePath}");
                Console.WriteLine($"Notes: {model.Notes}");
                Console.WriteLine($"Status: {model.Status}");
                Console.WriteLine($"AppliedDate: {model.AppliedDate}");
                Console.WriteLine($"=====================================");

                _context.JobApplications.Add(model);
                await _context.SaveChangesAsync();

                // Debug logging after saving
                Console.WriteLine($"=== AFTER SAVING APPLICATION ===");
                Console.WriteLine($"Application ID: {model.Id}");
                Console.WriteLine($"CoverLetter after save: {model.CoverLetter}");
                Console.WriteLine($"ResumePath after save: {model.ResumePath}");
                Console.WriteLine($"Notes after save: {model.Notes}");
                Console.WriteLine($"=====================================");

                // Get job details for success message
                var job = await _context.JobPostings
                    .Include(j => j.Employer)
                    .FirstOrDefaultAsync(j => j.Id == id);

                // Send email notifications
                try
                {
                    if (job != null)
                    {
                        Console.WriteLine($"Attempting to send emails...");
                        Console.WriteLine($"Job Seeker Email: {user.Email}");
                        Console.WriteLine($"Employer Email: {job.Employer.Email}");
                        
                        // Send confirmation email to job seeker
                        await _emailService.SendApplicationConfirmationAsync(
                            user.Email!,
                            $"{user.FirstName} {user.LastName}",
                            job.Title,
                            job.Company
                        );
                        Console.WriteLine($"Confirmation email sent to job seeker: {user.Email}");

                        // Send notification email to employer
                        await _emailService.SendNewApplicationNotificationAsync(
                            job.Employer.Email!,
                            $"{job.Employer.FirstName} {job.Employer.LastName}",
                            job.Title,
                            $"{user.FirstName} {user.LastName}"
                        );
                        Console.WriteLine($"Notification email sent to employer: {job.Employer.Email}");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the application
                    // In production, you might want to use a proper logging framework
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                    Console.WriteLine($"Email error details: {ex}");
                }

                // Set success message with job details
                if (job != null)
                {
                    TempData["SuccessMessage"] = $"Your application for '{job.Title}' at {job.Company} has been submitted successfully! You will receive a confirmation email shortly.";
                    TempData["JobTitle"] = job.Title;
                    TempData["CompanyName"] = job.Company;
                }
                else
                {
                    TempData["SuccessMessage"] = "Application submitted successfully! You will receive a confirmation email shortly.";
                }
                
                return RedirectToAction("MyApplications");
            }

            var jobForView = await _context.JobPostings
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.Id == id);

            ViewBag.Job = jobForView;
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var applications = await _context.JobApplications
                .Where(ja => ja.JobSeekerId == user.Id)
                .Include(ja => ja.JobPosting)
                .ThenInclude(jp => jp.Employer)
                .OrderByDescending(ja => ja.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

        private bool JobPostingExists(int id)
        {
            return _context.JobPostings.Any(e => e.Id == id);
        }

        // Test method to check database connectivity
        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var jobCount = await _context.JobPostings.CountAsync();
                var testJob = await _context.JobPostings.FirstOrDefaultAsync();
                
                return Json(new { 
                    success = true, 
                    jobCount = jobCount,
                    testJob = testJob != null ? new { id = testJob.Id, title = testJob.Title, company = testJob.Company } : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
