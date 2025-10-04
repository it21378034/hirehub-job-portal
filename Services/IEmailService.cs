using HireHub.Models;

namespace HireHub.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendApplicationConfirmationAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName);
        Task SendApplicationRejectionAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName);
        Task SendApplicationShortlistAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName);
        Task SendNewApplicationNotificationAsync(string employerEmail, string employerName, string jobTitle, string applicantName);
    }
}
