using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rental.Models;

namespace Rental.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // PUBLIC PAGE — Anyone can access
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
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
