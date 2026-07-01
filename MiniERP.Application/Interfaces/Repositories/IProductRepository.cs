using MiniERP.Domain.Entities;
using System.Linq.Expressions;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(Product product);
        void Update(Product product);
        void Delete(Product product);

        
        /// Check if product has inventory with quantity > 0.
        
        Task<bool> HasInventoryAsync(Guid productId, CancellationToken cancellationToken = default);

        
        /// Check if product has any stock transactions.
        
        Task<bool> HasStockTransactionsAsync(Guid productId, CancellationToken cancellationToken = default);

        
        /// Check if product appears in any order items (sales or purchase).
        
        Task<bool> HasOrderItemsAsync(Guid productId, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
