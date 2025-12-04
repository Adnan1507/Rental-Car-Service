using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rental.Models;
using Rental.UnitOfWork;
using Rental.ViewModels;

namespace Rental.Controllers
{
    public class CarController : Controller
    {
        private readonly IUnitofWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CarController>? _logger;

        public CarController(IUnitofWork unitOfWork,
                             UserManager<ApplicationUser> userManager,
                             IWebHostEnvironment env,
                             ILogger<CarController>? logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        // GET: /Car/Create
        [HttpGet]
        [Authorize(Roles = "Host")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
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
        [Authorize(Roles = "Host")]
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
        [Authorize(Roles = "Host")]
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

            var carToUpdate = await _unitOfWork.Cars.GetByIdAsync(id);
            if (carToUpdate == null) return NotFound();

            var userForEdit = await _userManager.GetUserAsync(User);
            if (userForEdit == null) return RedirectToAction("Login", "Account");

            if (carToUpdate.HostId != userForEdit.Id)
            {
                return Forbid();
            }

            // Update only allowed fields
            carToUpdate.PricePerDay = model.PricePerDay!.Value;
            carToUpdate.Location = model.Location!;
            carToUpdate.Description = model.Description;

            _unitOfWork.Cars.Update(carToUpdate);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("HostDashboard", "Home");
        }

        // POST: /Car/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
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

        // GET: /Car/Requests
        [HttpGet]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Requests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var requests = await _unitOfWork.Bookings.GetRequestsByHostAsync(user.Id);
            return View(requests);
        }

        // GET: /Car/Bookings
        [HttpGet]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Bookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var bookings = await _unitOfWork.Bookings.GetBookingsByHostAsync(user.Id);
            return View(bookings);
        }

        // POST: /Car/ApproveBooking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
            if (booking == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (booking.Car.HostId != user.Id) return Forbid();

            booking.Status = BookingStatus.Approved;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Requests));
        }

        // POST: /Car/RejectBooking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
            if (booking == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (booking.Car.HostId != user.Id) return Forbid();

            booking.Status = BookingStatus.Rejected;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Requests));
        }

        // GET: /Car/Rent/5
        [HttpGet]
        [Authorize(Roles = "Renter")]
        public async Task<IActionResult> Rent(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null || car.Status != CarStatus.Approved)
                return NotFound();

            // prevent owner from renting own car
            var user = await _userManager.GetUserAsync(User);
            if (user != null && car.HostId == user.Id)
                return Forbid();

            var vm = new BookingCreateViewModel
            {
                CarId = car.Id,
                CarTitle = $"{car.Brand} {car.Model}",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                PricePerDay = car.PricePerDay
            };

            return View(vm);
        }

        // POST: /Car/Rent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Renter")]
        public async Task<IActionResult> Rent(BookingCreateViewModel model)
        {
            _logger?.LogDebug("POST /Car/Rent called. Model: {@Model}", model);

            // Attempt to get car even if model binding failed so we can repopulate display fields
            var car = await _unitOfWork.Cars.GetByIdAsync(model.CarId);
            if (car == null || car.Status != CarStatus.Approved)
            {
                // If car not found, show 404
                return NotFound();
            }

            // Always ensure UI fields are set so the view shows correct info if we redisplay
            model.CarTitle = $"{car.Brand} {car.Model}";
            model.PricePerDay = car.PricePerDay;

            if (!ModelState.IsValid)
            {
                // If model binding failed (dates missing/invalid), recalc estimated total for UX
                try
                {
                    var daysPreview = (model.EndDate.Date - model.StartDate.Date).Days;
                    if (daysPreview < 1) daysPreview = 1;
                    model.TotalPrice = model.PricePerDay * (daysPreview > 0 ? daysPreview : 1);
                }
                catch
                {
                    model.TotalPrice = 0;
                }

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (car.HostId == user.Id)
                return Forbid();

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                // recalc for UX
                model.TotalPrice = model.PricePerDay;
                return View(model);
            }

            // availability check
            var hasOverlap = await _unitOfWork.Bookings.HasOverlappingBookingAsync(car.Id, model.StartDate, model.EndDate);
            if (hasOverlap)
            {
                ModelState.AddModelError("", "Selected dates are not available for this car. Please choose different dates.");
                // recompute estimate
                var days = (model.EndDate.Date - model.StartDate.Date).Days;
                if (days < 1) days = 1;
                model.TotalPrice = car.PricePerDay * days;
                return View(model);
            }

            // compute total server-side (trusted)
            var bookingDays = (model.EndDate.Date - model.StartDate.Date).Days;
            if (bookingDays < 1) bookingDays = 1;
            var totalPrice = car.PricePerDay * bookingDays;

            var booking = new Booking
            {
                CarId = car.Id,
                RenterId = user.Id,
                StartDate = model.StartDate.Date,
                EndDate = model.EndDate.Date,
                TotalPrice = totalPrice,
                Status = BookingStatus.Requested
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "Booking requested successfully.";
            return RedirectToAction("RenterDashboard", "Home");
        }
    }
}
