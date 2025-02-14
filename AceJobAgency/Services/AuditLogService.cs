using AceJobAgency.Model;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AceJobAgency.Services
{
    public class AuditLogService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _dbContext;

        public AuditLogService(UserManager<ApplicationUser> userManager, AuthDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task LogAsync(string userEmail, string action, string ipAddress = null)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var log = new AuditLog
                {
                    UserId = user.Id,
                    UserEmail = userEmail,
                    Action = action,
                    IPAddress = ipAddress ?? "Unknown", // ✅ Ensure a value is set
                    Timestamp = DateTime.UtcNow
                };

                _dbContext.AuditLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
