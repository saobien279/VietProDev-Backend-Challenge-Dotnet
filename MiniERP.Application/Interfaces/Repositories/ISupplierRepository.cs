using MiniERP.Domain.Entities;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Supplier, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(Supplier supplier);
        Task<bool> HasPurchaseOrdersAsync(Guid supplierId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}