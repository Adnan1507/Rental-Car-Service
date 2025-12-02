using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rental.Models;

namespace Rental.Data
{
    // This class runs when the application starts.
    // It creates default roles and a default admin user.
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(RoleManager<IdentityRole> roleManager,
                                               UserManager<ApplicationUser> userManager,
                                               ILogger? logger = null)
        {
            try
            {
                // 1. Ensure Admin Role Exists
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var r = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!r.Succeeded) logger?.LogError("Create role Admin failed: {Errors}", string.Join(", ", r.Errors.Select(e => e.Description)));
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

                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!createResult.Succeeded)
                    {
                        logger?.LogError("Creating admin user failed: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        return;
                    }

                    // 3. Assign Admin Role
                    var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addRoleResult.Succeeded)
                    {
                        logger?.LogError("Adding admin role failed: {Errors}", string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger?.LogInformation("Admin user created: {Email}", adminEmail);
                    }
                }
                else
                {
                    logger?.LogInformation("Admin already exists: {Email}", adminEmail);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "SeedAdminUser failed");
                throw;
            }
        }
    }
}
