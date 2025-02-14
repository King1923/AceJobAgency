using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AceJobAgency.Model;
using AceJobAgency.Services; // ✅ Added namespace for AuditLogService
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AceJobAgency.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly AuditLogService _auditLogService; // ✅ Added AuditLogService

        public LogoutModel(SignInManager<ApplicationUser> signInManager, AuditLogService auditLogService) // ✅ Injected AuditLogService
        {
            this.signInManager = signInManager;
            this._auditLogService = auditLogService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var userEmail = User.Identity?.Name;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            await signInManager.SignOutAsync(); // ✅ Use 'signInManager' instead of '_signInManager'

            if (!string.IsNullOrEmpty(userEmail))
            {
                await _auditLogService.LogAsync(userEmail, "Logout Successful", ipAddress);
            }

            return RedirectToPage("/Login");
        }

        public IActionResult OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }
    }
}