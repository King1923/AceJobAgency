using AceJobAgency.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Import logging
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AceJobAgency.Pages
{
    public class ForgetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgetPasswordModel> _logger; // Add logger

        public ForgetPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<ForgetPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger; // Initialize logger
        }

        [BindProperty]
        [Required, EmailAddress]
        public string Email { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ Model state is invalid.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                _logger.LogWarning($"⚠️ No user found with email: {Email}");
                return RedirectToPage("ForgetPasswordConfirm");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Page("/ResetPassword", null, new { email = Email, token }, Request.Scheme);

            _logger.LogInformation($"📨 Sending password reset email to {Email}");
            Console.WriteLine($"📨 Sending password reset email to {Email}");

            await _emailSender.SendEmailAsync(Email, "Reset Password", $"Click <a href='{resetLink}'>here</a> to reset your password.");
            _logger.LogInformation("✅ Email send function was called!");
            Console.WriteLine("✅ Email send function was called!");

            return RedirectToPage("ForgetPasswordConfirm");
        }
    }
}
