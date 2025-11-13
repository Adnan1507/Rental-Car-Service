using Rental.Models;
using Rental.Repository;

namespace Rental.UnitOfWork
{
    public class UnitOfWork : IUnitofWork
    {
        private readonly CompanyContext _context;

        public UnitOfWork(CompanyContext context)
        {
            _context = context;
            Companies = new CompanyRepository(_context);
        }

        public ICompanyRepository Companies { get; }

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
