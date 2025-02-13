using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.Tasks;
using AceJobAgency.Model;

namespace AceJobAgency.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataProtector _protector;

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string ResumeFilePath { get; set; }
        public string WhoAmI { get; set; }
        public string DecryptedNRIC { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager, IDataProtectionProvider provider)
        {
            _userManager = userManager;
            _protector = provider.CreateProtector("NRICProtection");
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                FirstName = user.FirstName;
                LastName = user.LastName;
                Email = user.Email;
                Gender = user.Gender;
                DateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd"); // Format Date of Birth
                ResumeFilePath = user.ResumeFilePath;
                WhoAmI = user.WhoAmI;
                DecryptedNRIC = _protector.Unprotect(user.NRIC);
            }
        }
    }
}