# HireHub - Job Portal Website

A comprehensive web-based job portal application built with ASP.NET MVC, SQLite database, and MDBootstrap for a modern, responsive user interface. HireHub connects talented job seekers with great employers worldwide.

## Features

### For Job Seekers
- User registration and profile management
- Browse and search job postings
- Apply for jobs with cover letters and resume uploads
- Track application status
- View application history

### For Employers
- Post job openings with detailed descriptions
- Manage job listings
- Review and manage applications
- Track application status
- Company profile management

### For Administrators
- User management (activate/deactivate users)
- Job posting management
- Application oversight
- System statistics and analytics
- Content moderation

## Technology Stack

- **Backend**: ASP.NET MVC 8.0
- **Database**: SQLite with Entity Framework Core
- **Frontend**: MDBootstrap (Material Design for Bootstrap)
- **Authentication**: ASP.NET Identity
- **Architecture**: MVC (Model-View-Controller)

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQLite (included with .NET)

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd job-portal
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update the database:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

5. Open your browser and navigate to `https://localhost:5001`

### Default Accounts

The application comes with pre-seeded accounts:

**Admin Account:**
- Email: admin@hirehub.com
- Password: Admin123!

**Sample Employer:**
- Email: employer@company.com
- Password: Employer123!

**Sample Job Seeker:**
- Email: jobseeker@email.com
- Password: JobSeeker123!

## Project Structure

```
HireHub/
├── Controllers/          # MVC Controllers
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── JobsController.cs
│   └── AdminController.cs
├── Models/              # Data Models and ViewModels
│   ├── ApplicationUser.cs
│   ├── JobPosting.cs
│   ├── JobApplication.cs
│   └── ViewModels/
├── Views/              # Razor Views
│   ├── Home/
│   ├── Account/
│   ├── Jobs/
│   ├── Admin/
│   └── Shared/
├── Data/               # Database Context and Seed Data
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── wwwroot/            # Static files (CSS, JS, images)
└── Program.cs          # Application entry point
```

## Key Features Implementation

### Authentication & Authorization
- Role-based access control (Admin, Employer, JobSeeker)
- Secure password policies
- Session management

### Job Management
- CRUD operations for job postings
- Advanced search and filtering
- Job status management
- Application tracking

### File Upload
- Resume upload functionality
- File type validation
- Secure file storage

### Responsive Design
- Mobile-first approach
- MDBootstrap components
- Modern UI/UX design

## Database Schema

### Users Table (ApplicationUser)
- User authentication and profile information
- Role-based access control
- Activity tracking

### JobPostings Table
- Job details and requirements
- Company information
- Salary and benefits
- Status management

### JobApplications Table
- Application tracking
- Cover letters and resumes
- Status updates
- Notes and comments

## Security Features

- Password hashing with ASP.NET Identity
- CSRF protection
- Input validation
- SQL injection prevention
- XSS protection

## Deployment

### Local Development
1. Ensure .NET 8.0 SDK is installed
2. Run `dotnet restore` to install packages
3. Run `dotnet ef database update` to create database
4. Run `dotnet run` to start the application

### Production Deployment
1. Configure connection strings for production database
2. Set up SSL certificates
3. Configure environment variables
4. Deploy to your preferred hosting platform

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please contact:
- Email: support@hirehub.com
- Documentation: [Link to documentation]

## Roadmap

- [ ] Email notifications
- [ ] Advanced search filters
- [ ] Job recommendations
- [ ] Company profiles
- [ ] Interview scheduling
- [ ] Analytics dashboard
- [ ] Mobile app
- [ ] API endpoints

## Changelog

### Version 1.0.0
- Initial release
- Basic job posting and application functionality
- User authentication and authorization
- Admin dashboard
- Responsive design with MDBootstrap
