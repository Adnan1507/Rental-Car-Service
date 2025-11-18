using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using System.Diagnostics;

namespace Rental.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        // PUBLIC PAGE — Anyone can access
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("AdminDashboard");

                if (await _userManager.IsInRoleAsync(user, "Host"))
                    return RedirectToAction("HostDashboard");

                if (await _userManager.IsInRoleAsync(user, "Renter"))
                    return RedirectToAction("RenterDashboard");
            }

            return View(); // normal homepage
        }


        // PUBLIC PAGE — Anyone can access
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // ===============================
        // DASHBOARD PAGES (Require Login)
        // ===============================

        // Only Admin can access
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // Only Host can access
        [Authorize(Roles = "Host")]
        public IActionResult HostDashboard()
        {
            return View();
        }

        // Only Renter can access
        [Authorize(Roles = "Renter")]
        public IActionResult RenterDashboard()
        {
            return View();
        }

        // ERROR HANDLING
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
