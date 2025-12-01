using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.UnitOfWork;
using Rental.ViewModels;

namespace Rental.Controllers
{
    [Authorize(Roles = "Host")]
    public class CarController : Controller
    {
        private readonly IUnitofWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CarController(IUnitofWork unitOfWork,
                             UserManager<ApplicationUser> userManager,
                             IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Car/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "")] CarCreateViewModel model)
        {
            // server-side required checks for nullable numeric properties
            if (model.Year == null)
            {
                ModelState.AddModelError(nameof(model.Year), "Year is required.");
            }
            if (model.Seats == null)
            {
                ModelState.AddModelError(nameof(model.Seats), "Seats is required.");
            }
            if (model.PricePerDay == null)
            {
                ModelState.AddModelError(nameof(model.PricePerDay), "Price per day is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // current logged-in Host
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // just in case
                return RedirectToAction("Login", "Account");
            }

            // save image
            string? imagePath = null;
            if (model.Image != null && model.Image.Length > 0)
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "cars");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = "/uploads/cars/" + fileName;
            }

            var car = new Car
            {
                HostId = user.Id,
                Brand = model.Brand!,
                CarType = model.CarType!,
                Model = model.Model!,
                Year = model.Year!.Value,
                Transmission = model.Transmission!,
                FuelType = model.FuelType!,
                Seats = model.Seats!.Value,
                PricePerDay = model.PricePerDay!.Value,
                Location = model.Location!,
                Description = model.Description,
                ImagePath = imagePath,
                Status = CarStatus.Pending
            };

            await _unitOfWork.Cars.AddAsync(car);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("HostDashboard", "Home");
        }

        // GET: /Car/Details/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var car = await _unitOfWork.Cars.GetCarWithHostAsync(id);
            if (car == null) return NotFound();

            return View(car);
        }

        // GET: /Car/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Only owner can edit
            if (car.HostId != user.Id)
            {
                return Forbid();
            }

            var vm = new CarEditViewModel
            {
                Id = car.Id,
                PricePerDay = car.PricePerDay,
                Location = car.Location,
                Description = car.Description,
                ExistingImagePath = car.ImagePath
            };

            return View(vm);
        }

        // POST: /Car/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // Validate required editable fields
            if (model.PricePerDay == null)
            {
                ModelState.AddModelError(nameof(model.PricePerDay), "Price per day is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(nameof(model.Location), "Location is required.");
            }

            if (!ModelState.IsValid)
            {
                // keep existing image path for display
                var carForImage = await _unitOfWork.Cars.GetByIdAsync(id);
                model.ExistingImagePath = carForImage?.ImagePath;
                return View(model);
            }

            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (car.HostId != user.Id)
            {
                return Forbid();
            }

            // Update only allowed fields
            car.PricePerDay = model.PricePerDay!.Value;
            car.Location = model.Location!;
            car.Description = model.Description;

            _unitOfWork.Cars.Update(car);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("HostDashboard", "Home");
        }

        // POST: /Car/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (car.HostId != user.Id)
            {
                return Forbid();
            }

            // delete image file if exists
            if (!string.IsNullOrEmpty(car.ImagePath))
            {
                var oldPath = car.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var oldFull = Path.Combine(_env.WebRootPath, oldPath);
                if (System.IO.File.Exists(oldFull))
                {
                    System.IO.File.Delete(oldFull);
                }
            }

            _unitOfWork.Cars.Delete(car);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("HostDashboard", "Home");
        }
    }
}
