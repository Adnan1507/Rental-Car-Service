using Rental.Models;

namespace Rental.Repository
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        // Requests where the bookable car belongs to the host and booking is Requested
        Task<IEnumerable<Booking>> GetRequestsByHostAsync(string hostId);

        // Confirmed / active bookings for host's cars
        Task<IEnumerable<Booking>> GetBookingsByHostAsync(string hostId);

        // Renter's past and upcoming bookings
        Task<IEnumerable<Booking>> GetBookingsByRenterAsync(string renterId);

        // Single booking with related car and renter
        Task<Booking?> GetBookingWithDetailsAsync(int id);

        // New: check whether a car already has a booking that overlaps the requested period
        Task<bool> HasOverlappingBookingAsync(int carId, DateTime startDate, DateTime endDate);
    }
}