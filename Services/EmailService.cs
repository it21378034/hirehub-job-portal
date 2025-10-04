using System.Net;
using System.Net.Mail;
using HireHub.Models;

namespace HireHub.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                Console.WriteLine($"=== EMAIL SENDING ATTEMPT ===");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"From: {_emailSettings.FromEmail} ({_emailSettings.FromName})");
                Console.WriteLine($"SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"Username: {_emailSettings.SmtpUsername}");
                Console.WriteLine($"Body Preview: {body.Substring(0, Math.Min(200, body.Length))}...");
                Console.WriteLine($"===============================");

                // For testing purposes, we'll simulate email sending
                // In production, uncomment the code below and provide real SMTP credentials
                Console.WriteLine($"✅ EMAIL SIMULATED SUCCESSFULLY (Test mode - not actually sent)");
                _logger.LogInformation($"Email simulated successfully to {toEmail}");
                
                // Simulate async operation
                await Task.Delay(100);
                
                /* Uncomment this section when you have real SMTP credentials:
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EMAIL SENDING FAILED: {ex.Message}");
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                // Don't throw in test mode to avoid breaking the application
                // throw;
            }
        }

        public async Task SendApplicationConfirmationAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName)
        {
            var subject = "Application Submitted Successfully - HireHub";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>Application Submitted!</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Thank you for applying through HireHub</p>
                        </div>
                        
                        <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Hello {jobSeekerName},</h2>
                            
                            <p>Your application has been successfully submitted for the following position:</p>
                            
                            <div style='background: white; padding: 20px; border-radius: 8px; border-left: 4px solid #667eea; margin: 20px 0;'>
                                <h3 style='color: #2c3e50; margin: 0 0 10px 0;'>{jobTitle}</h3>
                                <p style='color: #666; margin: 0;'><strong>Company:</strong> {companyName}</p>
                            </div>
                            
                            <p>We have received your application and it is now under review. The employer will be notified of your application and will contact you directly if you are selected for the next stage.</p>
                            
                            <div style='background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #2c3e50;'><strong>What's Next?</strong></p>
                                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                                    <li>The employer will review your application</li>
                                    <li>You may be contacted for an interview if selected</li>
                                    <li>You can track your application status in your profile</li>
                                </ul>
                            </div>
                            
                            <p>Thank you for using HireHub. We wish you the best of luck with your application!</p>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View My Applications</a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #666; font-size: 12px;'>
                            <p>This is an automated message from HireHub. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(jobSeekerEmail, subject, body);
        }

        public async Task SendApplicationRejectionAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName)
        {
            var subject = "Application Update - HireHub";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>Application Update</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Thank you for your interest</p>
                        </div>
                        
                        <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Hello {jobSeekerName},</h2>
                            
                            <p>Thank you for your interest in the following position:</p>
                            
                            <div style='background: white; padding: 20px; border-radius: 8px; border-left: 4px solid #e74c3c; margin: 20px 0;'>
                                <h3 style='color: #2c3e50; margin: 0 0 10px 0;'>{jobTitle}</h3>
                                <p style='color: #666; margin: 0;'><strong>Company:</strong> {companyName}</p>
                            </div>
                            
                            <p>After careful consideration, we regret to inform you that we have decided not to move forward with your application at this time.</p>
                            
                            <div style='background: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #856404;'><strong>Don't be discouraged!</strong></p>
                                <p style='margin: 10px 0 0 0; color: #856404;'>This decision was based on the specific requirements for this role. We encourage you to continue applying for other positions that match your skills and experience.</p>
                            </div>
                            
                            <p>We appreciate the time and effort you put into your application and wish you the best of luck in your job search.</p>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Browse More Jobs</a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #666; font-size: 12px;'>
                            <p>This is an automated message from HireHub. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(jobSeekerEmail, subject, body);
        }

        public async Task SendApplicationShortlistAsync(string jobSeekerEmail, string jobSeekerName, string jobTitle, string companyName)
        {
            var subject = "Great News! You've Been Shortlisted - HireHub";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>Congratulations! 🎉</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>You've been shortlisted for the next stage</p>
                        </div>
                        
                        <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Hello {jobSeekerName},</h2>
                            
                            <p>We're excited to inform you that your application has been shortlisted for the following position:</p>
                            
                            <div style='background: white; padding: 20px; border-radius: 8px; border-left: 4px solid #27ae60; margin: 20px 0;'>
                                <h3 style='color: #2c3e50; margin: 0 0 10px 0;'>{jobTitle}</h3>
                                <p style='color: #666; margin: 0;'><strong>Company:</strong> {companyName}</p>
                            </div>
                            
                            <p>Congratulations! Your qualifications and experience have impressed our hiring team. Our HR team will contact you soon to discuss the next steps in the hiring process.</p>
                            
                            <div style='background: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #155724;'><strong>What to Expect Next:</strong></p>
                                <ul style='margin: 10px 0 0 0; padding-left: 20px; color: #155724;'>
                                    <li>Our HR team will contact you within 2-3 business days</li>
                                    <li>You may be invited for an interview</li>
                                    <li>Further assessment or documentation may be required</li>
                                </ul>
                            </div>
                            
                            <p>Please keep your phone and email available for communication. We look forward to speaking with you soon!</p>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #27ae60; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Application Status</a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #666; font-size: 12px;'>
                            <p>This is an automated message from HireHub. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(jobSeekerEmail, subject, body);
        }

        public async Task SendNewApplicationNotificationAsync(string employerEmail, string employerName, string jobTitle, string applicantName)
        {
            var subject = "New Job Application Received - HireHub";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #3498db 0%, #2980b9 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>New Application Received!</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Someone has applied for your job posting</p>
                        </div>
                        
                        <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                            <h2 style='color: #2c3e50; margin-top: 0;'>Hello {employerName},</h2>
                            
                            <p>You have received a new job application for the following position:</p>
                            
                            <div style='background: white; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db; margin: 20px 0;'>
                                <h3 style='color: #2c3e50; margin: 0 0 10px 0;'>{jobTitle}</h3>
                                <p style='color: #666; margin: 0;'><strong>Applicant:</strong> {applicantName}</p>
                            </div>
                            
                            <p>Please log in to your employer dashboard to review the application details, including the candidate's resume and cover letter.</p>
                            
                            <div style='background: #e3f2fd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #1565c0;'><strong>Next Steps:</strong></p>
                                <ul style='margin: 10px 0 0 0; padding-left: 20px; color: #1565c0;'>
                                    <li>Review the candidate's application and resume</li>
                                    <li>Shortlist or reject the application</li>
                                    <li>Contact the candidate if interested</li>
                                </ul>
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Review Application</a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #666; font-size: 12px;'>
                            <p>This is an automated message from HireHub. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(employerEmail, subject, body);
        }
    }
}
