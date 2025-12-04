using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.UnitOfWork;
using Rental.ViewModels;

namespace Rental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitofWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IUnitofWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: /Admin/PendingCars on
        public async Task<IActionResult> PendingCars()
        {
            var all = await _unitOfWork.Cars.GetAllAsync();
            var pending = all.Where(c => c.Status == CarStatus.Pending)
                             .OrderByDescending(c => c.CreatedAt)
                             .ToList();

            var vm = new List<AdminPendingCarViewModel>();
            foreach (var car in pending)
            {
                var host = await _userManager.FindByIdAsync(car.HostId);
                var hostName = host?.FullName ?? host?.Email ?? car.HostId;
                vm.Add(new AdminPendingCarViewModel { Car = car, HostName = hostName });
            }

            return View(vm);
        }

        // POST: /Admin/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return NotFound();

            car.Status = CarStatus.Approved;
            _unitOfWork.Cars.Update(car);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(PendingCars));
        }

        // POST: /Admin/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return NotFound();

            car.Status = CarStatus.Rejected;
            _unitOfWork.Cars.Update(car);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(PendingCars));
        }

    }
}