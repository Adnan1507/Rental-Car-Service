using Rental.Models;

namespace Rental.Repository
{
    public interface ICarRepository : IBaseRepository<Car>
    {
        // All cars belonging to a specific host
        Task<IEnumerable<Car>> GetCarsByHostAsync(string hostId);

        // Approved cars for renter browsing
        Task<IEnumerable<Car>> GetApprovedCarsAsync();

        // A single car with its host info
        Task<Car?> GetCarWithHostAsync(int id);
    }
}
