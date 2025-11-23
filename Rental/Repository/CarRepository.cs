using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Repository
{
    public class CarRepository : BaseRepository<Car>, ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public CarRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Car>> GetCarsByHostAsync(string hostId)
        {
            return await _context.Cars
                .Where(c => c.HostId == hostId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetApprovedCarsAsync()
        {
            return await _context.Cars
                .Where(c => c.Status == CarStatus.Approved)
                .ToListAsync();
        }

        public async Task<Car?> GetCarWithHostAsync(int id)
        {
            return await _context.Cars
                .Include(c => c.Host)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
