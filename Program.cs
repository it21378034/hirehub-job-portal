using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HireHub.Data;
using HireHub.Models;
using HireHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

try
{
    // Add services to the container.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        // Force SQLite usage for this deployment
        options.UseSqlite(connectionString);
    });

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Simplified password settings for easier testing
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 3;
        options.Password.RequiredUniqueChars = 0;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Add MVC services
    builder.Services.AddControllersWithViews();

    // Add email service
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Configure email settings
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Health check endpoints
    app.MapGet("/health", () => "OK");
    app.MapGet("/test", () => new { Status = "OK", Message = "Working", Database = "SQLite" });

    // Initialize database before app starts
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting database initialization...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created successfully");

            // Apply migrations (ignore if tables already exist)
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully");
            }
            catch (Exception migrationEx)
            {
                logger.LogWarning($"Migration warning (may be expected): {migrationEx.Message}");
            }

            // Seed data if needed
            await SeedData.InitializeAsync(userManager, roleManager, context);
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            // Log the exception but don't stop the application
            Console.WriteLine($"Database initialization error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}