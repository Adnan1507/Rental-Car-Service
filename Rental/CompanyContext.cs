using Microsoft.EntityFrameworkCore;

namespace Rental.Models
{
        public class CompanyContext : DbContext
        {
            public CompanyContext(DbContextOptions<CompanyContext> options) : base(options)
            {
                
            }
            public DbSet<Rental.Models.Company> Company { get; set; }
    }
    
}
