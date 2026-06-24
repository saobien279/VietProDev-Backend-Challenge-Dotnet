using MiniERP.Domain.Entities;
using System.Linq.Expressions;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(Category category);
        void Update(Category category);
        void Delete(Category category);
        Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
