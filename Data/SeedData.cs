using Microsoft.AspNetCore.Identity;
using HireHub.Models;

namespace HireHub.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
        // Create roles if they don't exist
        try
        {
            string[] roles = { "Admin", "Employer", "JobSeeker" };
            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating roles: {ex.Message}");
        }

            // Create admin user if it doesn't exist
            if (await userManager.FindByEmailAsync("admin@hirehub.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@hirehub.com",
                    Email = "admin@hirehub.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create sample employer
            if (await userManager.FindByEmailAsync("employer@company.com") == null)
            {
                var employer = new ApplicationUser
                {
                    UserName = "employer@company.com",
                    Email = "employer@company.com",
                    FirstName = "John",
                    LastName = "Employer",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(employer, "Employer123!");
                await userManager.AddToRoleAsync(employer, "Employer");
            }

            // Create sample job seeker
            if (await userManager.FindByEmailAsync("jobseeker@email.com") == null)
            {
                var jobSeeker = new ApplicationUser
                {
                    UserName = "jobseeker@email.com",
                    Email = "jobseeker@email.com",
                    FirstName = "Jane",
                    LastName = "JobSeeker",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(jobSeeker, "JobSeeker123!");
                await userManager.AddToRoleAsync(jobSeeker, "JobSeeker");
            }

            // Create sample job postings
            if (!context.JobPostings.Any())
            {
                var employer = await userManager.FindByEmailAsync("employer@company.com");
                if (employer != null)
                {
                    var sampleJobs = new List<JobPosting>
                    {
                        new JobPosting
                        {
                            Title = "Senior Software Developer",
                            Company = "Tech Solutions Inc.",
                            Location = "New York, NY",
                            Description = "We are looking for an experienced software developer to join our team...",
                            JobType = "Full-time",
                            ExperienceLevel = "Senior",
                            SalaryMin = 80000,
                            SalaryMax = 120000,
                            SalaryCurrency = "USD",
                            Requirements = "5+ years of experience in C# and .NET, Experience with ASP.NET MVC, SQL Server knowledge",
                            Benefits = "Health insurance, 401k, Flexible working hours",
                            EmployerId = employer.Id,
                            PostedDate = DateTime.UtcNow.AddDays(-5),
                            ApplicationDeadline = DateTime.UtcNow.AddDays(30)
                        },
                        new JobPosting
                        {
                            Title = "Frontend Developer",
                            Company = "Digital Agency",
                            Location = "San Francisco, CA",
                            Description = "Join our creative team as a frontend developer...",
                            JobType = "Full-time",
                            ExperienceLevel = "Mid",
                            SalaryMin = 60000,
                            SalaryMax = 90000,
                            SalaryCurrency = "USD",
                            Requirements = "3+ years of experience with React, JavaScript, HTML/CSS",
                            Benefits = "Remote work, Health insurance, Professional development",
                            EmployerId = employer.Id,
                            PostedDate = DateTime.UtcNow.AddDays(-3),
                            ApplicationDeadline = DateTime.UtcNow.AddDays(25)
                        },
                        new JobPosting
                        {
                            Title = "Data Analyst",
                            Company = "Analytics Corp",
                            Location = "Chicago, IL",
                            Description = "We need a data analyst to help us make data-driven decisions...",
                            JobType = "Full-time",
                            ExperienceLevel = "Entry",
                            SalaryMin = 45000,
                            SalaryMax = 65000,
                            SalaryCurrency = "USD",
                            Requirements = "Bachelor's degree in Statistics, Mathematics, or related field, Experience with SQL and Excel",
                            Benefits = "Health insurance, Dental, Vision, 401k",
                            EmployerId = employer.Id,
                            PostedDate = DateTime.UtcNow.AddDays(-1),
                            ApplicationDeadline = DateTime.UtcNow.AddDays(20)
                        }
                    };

                    context.JobPostings.AddRange(sampleJobs);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
