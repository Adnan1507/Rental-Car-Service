using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Repository
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        private readonly CompanyContext _context;

        public CompanyRepository(CompanyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllCompanyAsync(string? searchTerm, int page = 1, int size = 5)
        {
            // Start from all companies
            var query = _context.Company.AsQueryable();

            // If searchTerm is not empty, filter by Name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            // Simple pagination + sorting
            return await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }
    }
}
