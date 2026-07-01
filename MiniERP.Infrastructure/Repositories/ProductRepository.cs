using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System.Linq.Expressions;

namespace MiniERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(predicate, cancellationToken);
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product); // DbContext intercepts → Soft Delete
        }

        public async Task<bool> HasInventoryAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories.AnyAsync(i => i.ProductId == productId && i.Quantity > 0, cancellationToken);
        }

        public async Task<bool> HasStockTransactionsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.StockTransactions.AnyAsync(st => st.ProductId == productId, cancellationToken);
        }

        public async Task<bool> HasOrderItemsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var hasPurchaseItems = await _context.PurchaseOrderItems.AnyAsync(poi => poi.ProductId == productId, cancellationToken);
            if (hasPurchaseItems) return true;

            return await _context.SalesOrderItems.AnyAsync(soi => soi.ProductId == productId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
