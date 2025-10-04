using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HireHub.Data;
using HireHub.Models;
using System.Linq;

namespace HireHub.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var profile = await _context.UserProfiles
                .Include(up => up.Skills)
                .Include(up => up.Education)
                .Include(up => up.Experience)
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (profile == null)
            {
                // Create a new profile if it doesn't exist
                profile = new UserProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }
            
            // Ensure profile has all includes loaded
            profile = await _context.UserProfiles
                .Include(up => up.Skills)
                .Include(up => up.Education)
                .Include(up => up.Experience)
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            // Get user roles for debugging
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.User = user;
            ViewBag.UserRoles = userRoles;
            
            
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (profile == null)
                {
                    // Create new profile
                    profile = new UserProfile
                    {
                        UserId = user.Id,
                        Bio = model.Bio,
                        CurrentPosition = model.CurrentPosition,
                        CurrentCompany = model.CurrentCompany,
                        Location = model.Location,
                        PhoneNumber = model.PhoneNumber,
                        Website = model.Website,
                        LinkedInProfile = model.LinkedInProfile,
                        GitHubProfile = model.GitHubProfile,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserProfiles.Add(profile);
                }
                else
                {
                    // Update existing profile
                    profile.Bio = model.Bio;
                    profile.CurrentPosition = model.CurrentPosition;
                    profile.CurrentCompany = model.CurrentCompany;
                    profile.Location = model.Location;
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.Website = model.Website;
                    profile.LinkedInProfile = model.LinkedInProfile;
                    profile.GitHubProfile = model.GitHubProfile;
                    profile.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                TempData["ProfileUpdateSuccess"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                // Log validation errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);
                Console.WriteLine($"Validation errors: {errorMessage}");
                
                TempData["ErrorMessage"] = $"Please correct the errors and try again. Errors: {errorMessage}";
                
                // Reload the profile to show the form with current data
                var currentProfile = await _context.UserProfiles
                    .Include(up => up.Skills)
                    .Include(up => up.Education)
                    .Include(up => up.Experience)
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);
                
                return View("Index", currentProfile);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResume(IFormFile resumeFile)
        {
            if (resumeFile == null || resumeFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Please upload a PDF, DOC, or DOCX file.";
                return RedirectToAction("Index");
            }

            // Validate file size (10MB limit)
            if (resumeFile.Length > 10 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File size must be less than 10MB.";
                return RedirectToAction("Index");
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(stream);
                }

                // Update user profile
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (profile == null)
                {
                    profile = new UserProfile
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserProfiles.Add(profile);
                }

                profile.ResumePath = $"/uploads/resumes/{fileName}";
                profile.ResumeFileName = resumeFile.FileName;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["ResumeUploadSuccess"] = "Resume uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(string skillName, string? proficiencyLevel, string? description)
        {
            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(skillName))
            {
                TempData["ErrorMessage"] = "Skill name is required.";
                return RedirectToAction("Index");
            }
            
            var model = new UserSkill
            {
                SkillName = skillName.Trim(),
                ProficiencyLevel = proficiencyLevel,
                Description = description
            };
            
            if (true) // Always proceed if skill name is provided
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found.";
                    return RedirectToAction("Index");
                }

                model.UserProfileId = profile.Id;
                _context.UserSkills.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Skill added successfully!";
            }
            else
            {
                // Debug: Show specific validation errors
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = string.Join(", ", x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>()) })
                    .ToList();
                
                var errorMessage = "Validation failed: " + string.Join("; ", errors.Select(e => $"{e.Field}: {e.Errors}"));
                TempData["ErrorMessage"] = errors.Any() ? errorMessage : "Please fill in all required fields.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEducation(string institution, string degree, string? fieldOfStudy, 
            DateTime? startDate, DateTime? endDate, bool isCurrentlyStudying, string? description)
        {
            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(institution))
            {
                TempData["ErrorMessage"] = "Institution is required.";
                return RedirectToAction("Index");
            }
            
            if (string.IsNullOrWhiteSpace(degree))
            {
                TempData["ErrorMessage"] = "Degree is required.";
                return RedirectToAction("Index");
            }
            
            var model = new UserEducation
            {
                Institution = institution.Trim(),
                Degree = degree.Trim(),
                FieldOfStudy = fieldOfStudy,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrentlyStudying = isCurrentlyStudying,
                Description = description
            };
            
            if (true) // Always proceed if required fields are provided
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found.";
                    return RedirectToAction("Index");
                }

                model.UserProfileId = profile.Id;
                _context.UserEducation.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Education added successfully!";
            }
            else
            {
                // Debug: Show specific validation errors
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = string.Join(", ", x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>()) })
                    .ToList();
                
                var errorMessage = "Validation failed: " + string.Join("; ", errors.Select(e => $"{e.Field}: {e.Errors}"));
                TempData["ErrorMessage"] = errors.Any() ? errorMessage : "Please fill in all required fields.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(string jobTitle, string company, string? location,
            DateTime startDate, DateTime? endDate, bool isCurrentlyWorking, string? description, string? achievements)
        {
            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                TempData["ErrorMessage"] = "Job title is required.";
                return RedirectToAction("Index");
            }
            
            if (string.IsNullOrWhiteSpace(company))
            {
                TempData["ErrorMessage"] = "Company is required.";
                return RedirectToAction("Index");
            }
            
            var model = new UserExperience
            {
                JobTitle = jobTitle.Trim(),
                Company = company.Trim(),
                Location = location,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrentlyWorking = isCurrentlyWorking,
                Description = description,
                Achievements = achievements
            };
            
            if (true) // Always proceed if required fields are provided
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found.";
                    return RedirectToAction("Index");
                }

                model.UserProfileId = profile.Id;
                _context.UserExperience.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Experience added successfully!";
            }
            else
            {
                // Debug: Show specific validation errors
                var errors = ModelState
                    .Where(x => x.Value?.Errors?.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = string.Join(", ", x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>()) })
                    .ToList();
                
                var errorMessage = "Validation failed: " + string.Join("; ", errors.Select(e => $"{e.Field}: {e.Errors}"));
                TempData["ErrorMessage"] = errors.Any() ? errorMessage : "Please fill in all required fields.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = await _context.UserSkills.FindAsync(id);
            if (skill != null)
            {
                _context.UserSkills.Remove(skill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Skill deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            var education = await _context.UserEducation.FindAsync(id);
            if (education != null)
            {
                _context.UserEducation.Remove(education);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Education deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            var experience = await _context.UserExperience.FindAsync(id);
            if (experience != null)
            {
                _context.UserExperience.Remove(experience);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Experience deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
