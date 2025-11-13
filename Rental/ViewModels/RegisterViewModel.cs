using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Rental.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Account Type")] // Renter or Host
        public string RoleType { get; set; }

        [Required]
        [Display(Name = "NID Picture")]
        public IFormFile NIDImage { get; set; }

        [Required]
        [Display(Name = "License Picture")]
        public IFormFile LicenseImage { get; set; }

        [Required]
        [Display(Name = "Profile Picture")]
        public IFormFile ProfileImage { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
