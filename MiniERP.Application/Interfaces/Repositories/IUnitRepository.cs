using MiniERP.Domain.Entities;
using System.Linq.Expressions;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IUnitRepository
    {
        Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Unit, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(Unit unit);
        void Update(Unit unit);
        void Delete(Unit unit);
        Task<bool> HasProductsAsync(int unitId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
