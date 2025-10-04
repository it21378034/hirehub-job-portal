using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HireHub.Models;

namespace HireHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<UserEducation> UserEducation { get; set; }
        public DbSet<UserExperience> UserExperience { get; set; }
        public DbSet<JobCategory> JobCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<JobPosting>()
                .HasOne(j => j.Employer)
                .WithMany(u => u.JobPostings)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobApplication>()
                .HasOne(ja => ja.JobPosting)
                .WithMany(jp => jp.Applications)
                .HasForeignKey(ja => ja.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobApplication>()
                .HasOne(ja => ja.JobSeeker)
                .WithMany(js => js.JobApplications)
                .HasForeignKey(ja => ja.JobSeekerId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Profile relationships
            builder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSkill>()
                .HasOne(us => us.UserProfile)
                .WithMany(up => up.Skills)
                .HasForeignKey(us => us.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserEducation>()
                .HasOne(ue => ue.UserProfile)
                .WithMany(up => up.Education)
                .HasForeignKey(ue => ue.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserExperience>()
                .HasOne(ue => ue.UserProfile)
                .WithMany(up => up.Experience)
                .HasForeignKey(ue => ue.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job Category relationships
            builder.Entity<JobPosting>()
                .HasOne(jp => jp.Category)
                .WithMany(jc => jc.JobPostings)
                .HasForeignKey(jp => jp.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes
            builder.Entity<JobPosting>()
                .HasIndex(j => j.Title);

            builder.Entity<JobPosting>()
                .HasIndex(j => j.Location);

            builder.Entity<JobPosting>()
                .HasIndex(j => j.Company);

            builder.Entity<JobPosting>()
                .HasIndex(j => j.Status);

            builder.Entity<JobApplication>()
                .HasIndex(ja => ja.Status);

            builder.Entity<JobCategory>()
                .HasIndex(jc => jc.Name);
        }
    }
}
