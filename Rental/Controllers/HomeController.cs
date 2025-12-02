using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.UnitOfWork;
using Rental.ViewModels;
using System.Diagnostics;

namespace Rental.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitofWork _unitOfWork;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            IUnitofWork unitOfWork)
        {
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        // PUBLIC PAGE — Anyone can access
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("AdminDashboard");

                    if (await _userManager.IsInRoleAsync(user, "Host"))
                        return RedirectToAction("HostDashboard");

                    if (await _userManager.IsInRoleAsync(user, "Renter"))
                        return RedirectToAction("RenterDashboard");
                }
            }
            // inside Index after loading cars
            var cars = await _unitOfWork.Cars.GetApprovedCarsAsync();
            return View(cars);

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

        // Host dashboard — show cars that belong to the logged-in host
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> HostDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var hostCars = await _unitOfWork.Cars.GetCarsByHostAsync(user.Id);
            return View(hostCars);
        }

        // Only Renter can access — show renter's bookings and available cars
        [Authorize(Roles = "Renter")]
        public async Task<IActionResult> RenterDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _unitOfWork.Bookings.GetBookingsByRenterAsync(user.Id);
            var availableCars = await _unitOfWork.Cars.GetApprovedCarsAsync();

            var vm = new Rental.ViewModels.RenterDashboardViewModel
            {
                Bookings = bookings,
                AvailableCars = availableCars
            };

            return View(vm);
        }

        // ERROR HANDLING
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
