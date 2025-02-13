using AceJobAgency.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AceJobAgency.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public void OnGet(string token, string email)
        {
            Token = token;
            Email = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null) return RedirectToPage("ResetPasswordConfirm");

            // Ensure new password is strong and different from the current password
            if (!IsPasswordStrong(NewPassword, user))
            {
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, Token, NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            return RedirectToPage("ResetPasswordConfirm");
        }

        private bool IsPasswordStrong(string password, ApplicationUser user)
        {
            List<string> errors = new List<string>();

            var currentPasswordValid = _userManager.CheckPasswordAsync(user, password).Result;
            if (currentPasswordValid)
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
                ModelState.AddModelError("NewPassword", string.Join(" ", errors));
                return false;
            }

            return true;
        }
    }
}