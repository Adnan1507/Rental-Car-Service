using Microsoft.AspNetCore.Identity;

namespace Rental.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; } // Renter only

        public string? NIDImagePath { get; set; }
        public string? LicenseImagePath { get; set; }
        public string? ProfileImagePath { get; set; }


        public string RoleType { get; set; } // "Renter" or "Host"
    }
}
