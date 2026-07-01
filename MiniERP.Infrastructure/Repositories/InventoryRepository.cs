using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Domain.Entities;
using MiniERP.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync(cancellationToken);
        }

        public void Add(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
        }

        public void Update(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
