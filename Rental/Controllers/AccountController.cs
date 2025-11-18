using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.ViewModels;
using Rental.Services;


namespace Rental.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _env = env;
            _emailSender = emailSender;
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
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = false,   // MUST be session-only
                ExpiresUtc = null
            };

            await _signInManager.SignInAsync(user, authProperties);


            // Redirect based on role
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("AdminDashboard", "Home");

            if (await _userManager.IsInRoleAsync(user, "Host"))
                return RedirectToAction("HostDashboard", "Home");

            if (await _userManager.IsInRoleAsync(user, "Renter"))
                return RedirectToAction("RenterDashboard", "Home");

            // fallback
            return RedirectToAction("Index", "Home");

        }

        // Shows the login page to the user
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("AdminDashboard", "Home");

                if (User.IsInRole("Host"))
                    return RedirectToAction("HostDashboard", "Home");

                if (User.IsInRole("Renter"))
                    return RedirectToAction("RenterDashboard", "Home");
            }

            return View();
        }


        // Handles login form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // IMPORTANT: Clear old cookies to avoid persistence issues
            await _signInManager.SignOutAsync();

            // Build custom authentication properties
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(14) : null
            };

            // Manually verify password
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Manual sign-in with correct persistence behavior
                await _signInManager.SignInAsync(user, authProperties);

                // Redirect to correct dashboard
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("AdminDashboard", "Home");

                if (await _userManager.IsInRoleAsync(user, "Host"))
                    return RedirectToAction("HostDashboard", "Home");

                if (await _userManager.IsInRoleAsync(user, "Renter"))
                    return RedirectToAction("RenterDashboard", "Home");

                return RedirectToAction("Index", "Home");
            }

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

            // Try to find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // If email is not registered, show error instead of redirecting
            if (user == null)
            {
                ModelState.AddModelError("", "This email is not registered.");
                return View(model);
            }

            // Generate token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create reset link
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            // Send email
            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a>");

            // Go to confirmation page
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

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }



    }
}
