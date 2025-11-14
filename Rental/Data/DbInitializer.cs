using Microsoft.AspNetCore.Identity;
using Rental.Models;

namespace Rental.Data
{
    // This class runs when the application starts.
    // It creates default roles and a default admin user.
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(RoleManager<IdentityRole> roleManager,
                                               UserManager<ApplicationUser> userManager)
        {
            // 1. Ensure Admin Role Exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Create Default Admin User
            var adminEmail = "admin@rental.com";
            var adminPassword = "Admin@123";  // You can change later

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    Address = "System Headquarters",
                    RoleType = "Admin",
                    NIDImagePath = "",
                    LicenseImagePath = "",
                    ProfileImagePath = ""
                };

                await userManager.CreateAsync(adminUser, adminPassword);

                // 3. Assign Admin Role
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
