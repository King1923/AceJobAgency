using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AceJobAgency.Model;

namespace AceJobAgency.Pages
{
    public class OTPVerificationModel : PageModel
    {
        [BindProperty]
        public string OTPInput { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public OTPVerificationModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            string storedOTP = HttpContext.Session.GetString("OTP");
            string otpExpiryStr = HttpContext.Session.GetString("OTPExpiry");
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(otpExpiryStr))
            {
                ModelState.AddModelError(string.Empty, "OTP expired. Please request a new one.");
                return Page();
            }

            DateTime otpExpiry = DateTime.Parse(otpExpiryStr);
            if (DateTime.UtcNow > otpExpiry)
            {
                ModelState.AddModelError(string.Empty, "OTP has expired. Please request a new one.");
                return Page();
            }

            if (OTPInput != storedOTP)
            {
                ModelState.AddModelError(string.Empty, "Incorrect OTP. Please try again.");
                return Page();
            }

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            await signInManager.SignInAsync(user, isPersistent: true);

            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTPExpiry");
            HttpContext.Session.Remove("UserEmail");

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnGetResendOTPAsync()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Login");
            }

            string otp = GenerateOTP();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTPExpiry", DateTime.UtcNow.AddMinutes(1).ToString());

            await emailSender.SendEmailAsync(userEmail, "Your New OTP Code", $"Your new OTP is: <b>{otp}</b>. It expires in 1 minute.");

            return Page();
        }

        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
