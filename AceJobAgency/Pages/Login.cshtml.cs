using AceJobAgency.ViewModels;
using AceJobAgency.Model;
using AceJobAgency.Services; // ✅ Added namespace for AuditLogService
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AceJobAgency.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        [BindProperty]
        public string RecaptchaToken { get; set; }  // reCAPTCHA Token

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly HttpClient _httpClient; // HttpClient for reCAPTCHA verification
        private readonly IEmailSender emailSender; // Email service for OTP
        private readonly AuditLogService _auditLogService; // ✅ Added AuditLogService

        private const int MaxFailedAttempts = 3;
        private const double SuspiciousThreshold = 0.5; // Threshold for suspicious activity
        private const string RecaptchaSecretKey = "6LdUSdYqAAAAABW4zay5Z9cfPPVO1MKIDBRYb8m6"; // Replace with your actual secret key

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          HttpClient httpClient,
                          IEmailSender emailSender,
                          AuditLogService auditLogService) // ✅ Injected AuditLogService
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this._httpClient = httpClient;
            this.emailSender = emailSender;
            this._auditLogService = auditLogService; // ✅ Assign AuditLogService
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Console.WriteLine($"[DEBUG] Received reCAPTCHA Token: {RecaptchaToken}");

            // Verify reCAPTCHA
            var (isRecaptchaValid, recaptchaScore) = await VerifyRecaptchaAsync(RecaptchaToken);
            Console.WriteLine($"[DEBUG] reCAPTCHA Valid: {isRecaptchaValid}, Score: {recaptchaScore}");

            if (!isRecaptchaValid)
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA validation failed. Please try again.");
                return Page();
            }

            if (recaptchaScore < SuspiciousThreshold)
            {
                Console.WriteLine("[DEBUG] Suspicious activity detected.");
                ModelState.AddModelError(string.Empty, "Your activity looks suspicious. Please wait a few minutes before trying again.");
                return Page();
            }

            var user = await userManager.FindByEmailAsync(LModel.Email);
            if (user == null)
            {
                Console.WriteLine("[DEBUG] User not found.");
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                Console.WriteLine("[DEBUG] Account is locked out.");
                ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                return Page();
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, LModel.Password);
            Console.WriteLine($"[DEBUG] Password valid: {passwordValid}");
            if (!passwordValid)
            {
                await userManager.AccessFailedAsync(user);
                int failedAttempts = await userManager.GetAccessFailedCountAsync(user);
                Console.WriteLine($"[DEBUG] Failed Attempts: {failedAttempts}");

                if (failedAttempts >= MaxFailedAttempts)
                {
                    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(1));
                    ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }

                return Page();
            }

            await userManager.ResetAccessFailedCountAsync(user);
            HttpContext.Session.Remove("FailedLoginAttempts");

            // ✅ Log successful login
            await _auditLogService.LogAsync(user.Email, "Login Successful");

            // ✅ Generate OTP
            string otp = GenerateOTP();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTPExpiry", DateTime.UtcNow.AddMinutes(3).ToString());
            HttpContext.Session.SetString("UserEmail", LModel.Email);

            // ✅ Send OTP via Email
            await emailSender.SendEmailAsync(LModel.Email, "Your OTP Code", $"Your OTP is: <b>{otp}</b>. It expires in 1 minute.");

            // ✅ Redirect to OTP Verification Page
            return RedirectToPage("/OTPVerification");
        }

        private async Task<(bool success, double score)> VerifyRecaptchaAsync(string recaptchaToken)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={RecaptchaSecretKey}&response={recaptchaToken}",
                    null);

                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] reCAPTCHA response: {result}");

                JObject jsonResult = JObject.Parse(result);
                bool success = jsonResult["success"]?.Value<bool>() ?? false;
                double score = jsonResult["score"]?.Value<double>() ?? 0.0;

                return (success, score);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] reCAPTCHA verification error: {ex.Message}");
                return (false, 0.0);
            }
        }

        private string GenerateOTP()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[4];
                rng.GetBytes(tokenData);
                int otp = BitConverter.ToUInt16(tokenData, 0) % 1000000;
                return otp.ToString("D6"); // 6-digit OTP
            }
        }
    }
}