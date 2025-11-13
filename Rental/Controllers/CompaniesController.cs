using Microsoft.AspNetCore.Mvc;
using Rental.Models;
using Rental.UnitOfWork;

namespace Rental.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly IUnitofWork _unitOfWork;

        public CompaniesController(IUnitofWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            var companies = await _unitOfWork.Companies.GetAllAsync();
            return View(companies);
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var company = await _unitOfWork.Companies.GetByIdAsync(id.Value);
            if (company == null) return NotFound();

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Companies.AddAsync(company);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _unitOfWork.Companies.GetByIdAsync(id.Value);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company)
        {
            if (id != company.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _unitOfWork.Companies.Update(company);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var company = await _unitOfWork.Companies.GetByIdAsync(id.Value);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company != null)
            {
                _unitOfWork.Companies.Delete(company);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
