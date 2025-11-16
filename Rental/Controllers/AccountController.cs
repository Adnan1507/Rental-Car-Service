using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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

        // Shows the login page to the user
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handles login form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Step 1: Check if form values follow validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 2: Try to find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Show generic error (avoid telling user that email does not exist)
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Step 3: Check if password is correct
            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            // Step 4: Successfully logged in → Redirect based on role
            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("AdminDashboard", "Home");

                if (await _userManager.IsInRoleAsync(user, "Host"))
                    return RedirectToAction("HostDashboard", "Home");

                if (await _userManager.IsInRoleAsync(user, "Renter"))
                    return RedirectToAction("RenterDashboard", "Home");

                // Default fallback if somehow role missing
                return RedirectToAction("Index", "Home");
            }

            // Step 5: Wrong password or login failed
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // Logs the user out and redirects to Home page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a>");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("Invalid password reset token");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

    }
}
