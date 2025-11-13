using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.ViewModels;

namespace Rental.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _env = env;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.RoleType != "Renter" && model.RoleType != "Host")
            {
                ModelState.AddModelError("RoleType", "Please choose Renter or Host.");
                return View(model);
            }

            // Create the user WITHOUT images first
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                Address = model.Address,
                RoleType = model.RoleType
            };

            // Try creating user
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Show errors and DO NOT save any image
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Only save images AFTER the user is successfully created
            var nidPath = await SaveFileAsync(model.NIDImage, "nid");
            var licensePath = await SaveFileAsync(model.LicenseImage, "license");
            var profilePath = await SaveFileAsync(model.ProfileImage, "profile");

            // Update user with image paths
            user.NIDImagePath = nidPath;
            user.LicenseImagePath = licensePath;
            user.ProfileImagePath = profilePath;

            // Save update
            await _userManager.UpdateAsync(user);

            // Create role if not exist
            if (!await _roleManager.RoleExistsAsync(model.RoleType))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.RoleType));
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, model.RoleType);

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }


        // Helper: Save uploaded file and return relative path
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "users", subFolder);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the path usable in <img src="...">
            return $"/uploads/users/{subFolder}/{fileName}";
        }
    }
}
