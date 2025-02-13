using AceJobAgency.ViewModels;
using AceJobAgency.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AceJobAgency.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        private const int MaxFailedAttempts = 3;
        private const int MaxFailedAttemptsForUsers = 3;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await Task.Delay(TimeSpan.FromSeconds(5)); // Introduce delay to slow down brute-force attacks

            var sanitizedEmail = HtmlEncoder.Default.Encode(LModel.Email);
            var user = await userManager.FindByEmailAsync(sanitizedEmail);

            if (user == null)
            {
                int failedAttempts = HttpContext.Session.GetInt32("FailedLoginAttempts") ?? 0;
                failedAttempts++;
                HttpContext.Session.SetInt32("FailedLoginAttempts", failedAttempts);

                if (failedAttempts >= MaxFailedAttempts)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Invalid username or password. (Failed attempts: {failedAttempts})");
                return Page();
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                return Page();
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, LModel.Password);
            if (!passwordValid)
            {
                await userManager.AccessFailedAsync(user);
                int failedAttempts = await userManager.GetAccessFailedCountAsync(user);

                if (failedAttempts >= MaxFailedAttemptsForUsers)
                {
                    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
                    ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Invalid username or password. (Failed attempts: {failedAttempts})");
                }

                return Page();
            }

            await userManager.ResetAccessFailedCountAsync(user);
            HttpContext.Session.Remove("FailedLoginAttempts");

            var identityResult = await signInManager.PasswordSignInAsync(user.UserName, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

            if (identityResult.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            if (identityResult.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed attempts. Please try again later.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }
    }
}
