using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MiniERP.Application.DTOs.PurchaseOrders;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync(Guid? createdByFilter = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(PurchaseOrderQueryDto query, CancellationToken cancellationToken = default);
        Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(PurchaseOrder purchaseOrder);
        void Update(PurchaseOrder purchaseOrder);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
