using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireHub.Data;
using HireHub.Models;

namespace HireHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get recent job postings for the homepage
            var recentJobs = await _context.JobPostings
                .Where(j => j.Status == "Active")
                .OrderByDescending(j => j.PostedDate)
                .Take(6)
                .ToListAsync();

            ViewBag.RecentJobs = recentJobs;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Search(string? keyword, string? location, string? jobType)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Location = location;
            ViewBag.JobType = jobType;
            return View();
        }
    }
}