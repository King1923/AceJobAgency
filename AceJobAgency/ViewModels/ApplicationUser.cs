using Microsoft.AspNetCore.Identity;

namespace AceJobAgency.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string NRIC { get; set; }  // This will be encrypted
        public DateTime DateOfBirth { get; set; }
        public string WhoAmI { get; set; }
        public string ResumeFilePath { get; set; } // Path to resume file
    }
}