using Rental.Models;
using Rental.Repository;

namespace Rental.UnitOfWork
{
    public class UnitOfWork : IUnitofWork
    {
        private readonly ApplicationDbContext _context;

        public ICompanyRepository Companies { get; }
        public ICarRepository Cars { get; }
        public IBookingRepository Bookings { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Companies = new CompanyRepository(_context);
            Cars = new CarRepository(_context);
            Bookings = new BookingRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
