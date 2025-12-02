using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Repository
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetRequestsByHostAsync(string hostId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Where(b => b.Car.HostId == hostId && b.Status == BookingStatus.Requested)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByHostAsync(string hostId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Where(b => b.Car.HostId == hostId && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.Active))
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByRenterAsync(string renterId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Where(b => b.RenterId == renterId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // New: returns true if any existing booking for the car overlaps [startDate, endDate]
        public async Task<bool> HasOverlappingBookingAsync(int carId, DateTime startDate, DateTime endDate)
        {
            // Normalize dates to date-only to avoid partial-day confusion
            var start = startDate.Date;
            var end = endDate.Date;

            // Exclude clearly non-conflicting statuses (Rejected, Cancelled, Completed)
            var conflicting = await _context.Bookings
                .Where(b => b.CarId == carId
                            && b.Status != BookingStatus.Rejected
                            && b.Status != BookingStatus.Cancelled
                            && b.Status != BookingStatus.Completed)
                // overlap condition: existing.End >= requested.Start && existing.Start <= requested.End
                .AnyAsync(b => b.EndDate.Date >= start && b.StartDate.Date <= end);

            return conflicting;
        }
    }
}