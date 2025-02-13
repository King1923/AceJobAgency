using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AceJobAgency.Model;

namespace AceJobAgency.Pages
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePasswordViewModel CModel { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(CModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, CModel.CurrentPassword);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Incorrect current password.");
                return Page();
            }

            if (CModel.NewPassword != CModel.ConfirmNewPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirmation do not match.");
                return Page();
            }

            // Validate new password strength
            if (!IsPasswordStrong(CModel.NewPassword, CModel.CurrentPassword))
            {
                return Page();
            }

            var result = await _userManager.ChangePasswordAsync(user, CModel.CurrentPassword, CModel.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.SignOutAsync(); // Logout user after password change
            return RedirectToPage("/Login");
        }


        private bool IsPasswordStrong(string password, string currentPassword)
        {
            List<string> errors = new List<string>();

            if (password == currentPassword)
                errors.Add("New password must not match the current password.");

            if (password.Length < 12)
                errors.Add("Password must be at least 12 characters long.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must include at least one uppercase letter.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must include at least one lowercase letter.");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("Password must include at least one number.");

            if (!Regex.IsMatch(password, @"[\W_]"))
                errors.Add("Password must include at least one special character.");

            if (errors.Count > 0)
            {
                ModelState.AddModelError("CModel.NewPassword", string.Join(" ", errors));
                return false;
            }

            return true;
        }
    }

    public class ChangePasswordViewModel
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
