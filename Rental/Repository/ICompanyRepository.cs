using Rental.Models;

namespace Rental.Repository
{
    // Extra methods specific to Company (on top of basic CRUD)
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<List<Company>> GetAllCompanyAsync(string? searchTerm, int page = 1, int size = 5);
    }
}
