using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;

namespace AceJobAgency.ViewModels
{
    public class Register
    {
        [Required]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[A-Za-z\s]{2,}$", ErrorMessage = "First Name must contain only letters and spaces, and be at least 2 characters long.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[A-Za-z\s]{2,}$", ErrorMessage = "Last Name must contain only letters and spaces, and be at least 2 characters long.")]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "NRIC")]
        [RegularExpression(@"^[A-Za-z0-9]{1,9}$", ErrorMessage = "NRIC must contain only letters and numbers, max 9 characters.")]
        public string NRIC { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Resume")]
        public IFormFile Resume { get; set; }

        [Required]
        [Display(Name = "Who Am I")]
        public string WhoAmI { get; set; }

        // Encrypt NRIC before storing in DB
        public string EncryptNRIC(IDataProtector protector)
        {
            return protector.Protect(NRIC);
        }
    }
}
