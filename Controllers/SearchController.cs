using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireHub.Data;
using HireHub.Models;

namespace HireHub.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Jobs(string? keyword, string? location, string? jobType, string? category, 
            decimal? minSalary, decimal? maxSalary, string? experienceLevel, string? company)
        {
            // Force a complete context refresh to ensure we get the latest data
            _context.ChangeTracker.Clear();
            
            var query = _context.JobPostings
                .Where(j => j.IsActive && j.Status == "Approved")
                .Include(j => j.Employer)
                .Include(j => j.Category)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || 
                                        j.Description.Contains(keyword) || 
                                        j.Company.Contains(keyword) ||
                                        j.Requirements.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(j => j.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(j => j.JobType == jobType);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(j => j.Category != null && j.Category.Name == category);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(j => j.SalaryMin >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(j => j.SalaryMax <= maxSalary.Value);
            }

            if (!string.IsNullOrEmpty(experienceLevel))
            {
                query = query.Where(j => j.ExperienceLevel == experienceLevel);
            }

            if (!string.IsNullOrEmpty(company))
            {
                query = query.Where(j => j.Company.Contains(company));
            }

            var jobs = await query
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            // Get filter options for the view
            var categories = await _context.JobCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var companies = await _context.JobPostings
                .Where(j => j.IsActive)
                .Select(j => j.Company)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Companies = companies;
            ViewBag.Keyword = keyword;
            ViewBag.Location = location;
            ViewBag.JobType = jobType;
            ViewBag.Category = category;
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;
            ViewBag.ExperienceLevel = experienceLevel;
            ViewBag.Company = company;

            return View(jobs);
        }

        [HttpGet]
        public async Task<IActionResult> Candidates(string? keyword, string? location, string? skills, string? experience)
        {
            // Force a complete context refresh to ensure we get the latest data
            _context.ChangeTracker.Clear();
            
            var query = _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.Skills)
                .Include(up => up.Experience)
                .Where(up => up.User.IsActive)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(up => (up.Bio != null && up.Bio.Contains(keyword)) || 
                                        (up.CurrentPosition != null && up.CurrentPosition.Contains(keyword)) ||
                                        (up.CurrentCompany != null && up.CurrentCompany.Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(up => up.Location != null && up.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(skills))
            {
                query = query.Where(up => up.Skills.Any(s => s.SkillName != null && s.SkillName.Contains(skills)));
            }

            if (!string.IsNullOrEmpty(experience))
            {
                // Filter by years of experience (simplified)
                query = query.Where(up => up.Experience.Any(e => e.JobTitle != null && e.JobTitle.Contains(experience)));
            }

            var candidates = await query
                .OrderByDescending(up => up.UpdatedAt)
                .ToListAsync();

            ViewBag.Keyword = keyword ?? "";
            ViewBag.Location = location ?? "";
            ViewBag.Skills = skills ?? "";
            ViewBag.Experience = experience ?? "";

            return View(candidates);
        }
    }
}
