using MiniERP.Domain.Entities; 

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(Customer customer);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}