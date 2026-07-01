using MiniERP.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        void Add(Inventory inventory);
        void Update(Inventory inventory);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
