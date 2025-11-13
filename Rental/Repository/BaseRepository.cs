using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Repository
{
    // This is a base class that implements the common CRUD logic
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly CompanyContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(CompanyContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }
    }
}
