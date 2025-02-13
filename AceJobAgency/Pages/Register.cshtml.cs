using AceJobAgency.ViewModels;
using AceJobAgency.Model; // Import ApplicationUser
using Microsoft.AspNetCore.DataProtection; // Import Data Protection
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web; // Import for XSS prevention

namespace AceJobAgency.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IDataProtector _protector;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             IDataProtectionProvider provider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _protector = provider.CreateProtector("NRICProtection");
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = await userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("RModel.Email", "Email already exists.");
                return Page();
            }

            var sanitizedFirstName = HtmlEncoder.Default.Encode(RModel.FirstName);
            var sanitizedLastName = HtmlEncoder.Default.Encode(RModel.LastName);
            var sanitizedWhoAmI = HtmlEncoder.Default.Encode(RModel.WhoAmI);
            var sanitizedEmail = HtmlEncoder.Default.Encode(RModel.Email);
            var encryptedNRIC = _protector.Protect(RModel.NRIC);

            // Save resume file and get the file path
            string resumePath = await SaveResumeAsync(RModel.Resume);
            if (resumePath == null)
            {
                return Page(); // Errors are already added in SaveResumeAsync method
            }

            var user = new ApplicationUser()
            {
                UserName = sanitizedEmail,
                Email = sanitizedEmail,
                FirstName = sanitizedFirstName,
                LastName = sanitizedLastName,
                Gender = RModel.Gender,
                NRIC = encryptedNRIC,
                DateOfBirth = RModel.DateOfBirth,
                WhoAmI = sanitizedWhoAmI,
                ResumeFilePath = resumePath
            };

            var result = await userManager.CreateAsync(user, RModel.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return Page();
        }

        private async Task<string> SaveResumeAsync(IFormFile resume)
        {
            if (resume == null || resume.Length == 0)
            {
                ModelState.AddModelError("RModel.Resume", "The Resume field is required.");
                return null;
            }

            var allowedExtensions = new[] { ".pdf", ".docx" };
            var fileExtension = Path.GetExtension(resume.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("RModel.Resume", "Invalid file type. Only PDF or DOCX files are allowed.");
                return null;
            }

            if (resume.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("RModel.Resume", "File size must be less than 2MB.");
                return null;
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await resume.CopyToAsync(fileStream);
            }

            return filePath;
        }

        private bool IsPasswordStrong(string password)
        {
            List<string> errors = new List<string>();

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
                ModelState.AddModelError("RModel.Password", string.Join(" ", errors));
                return false;
            }

            return true;
        }
    }
}