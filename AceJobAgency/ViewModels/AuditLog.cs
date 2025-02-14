using System;

namespace AceJobAgency.Model
{
    public class AuditLog
    {
        public int Id { get; set; } // Primary Key
        public string UserId { get; set; } // Store User ID (FK)
        public string UserEmail { get; set; } // Store user email
        public string Action { get; set; } // Example: "Login", "Logout"
        public string IPAddress { get; set; } // Capture user's IP
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
