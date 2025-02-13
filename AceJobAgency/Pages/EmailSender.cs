using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            _logger.LogInformation($"📩 Attempting to send email to {email}...");

            var smtpClient = new SmtpClient
            {
                Host = _config["SmtpSettings:Host"],
                Port = int.Parse(_config["SmtpSettings:Port"]),
                Credentials = new NetworkCredential(
                    _config["SmtpSettings:Username"],
                    _config["SmtpSettings:Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["SmtpSettings:Username"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email sent successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error sending email: {ex.Message}");
        }
    }
}
