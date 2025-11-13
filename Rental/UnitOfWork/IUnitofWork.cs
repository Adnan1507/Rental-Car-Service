using Rental.Repository;

namespace Rental.UnitOfWork
{
    // Unit of Work coordinates saving changes for multiple repositories
    public interface IUnitofWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        Task<int> CompleteAsync(); // Save changes to database
    }
}
