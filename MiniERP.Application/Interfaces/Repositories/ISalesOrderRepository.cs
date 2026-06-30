using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniERP.Application.DTOs.SalesOrders;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrder>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<SalesOrder> Items, int TotalCount)> GetPagedAsync(SalesOrderQueryDto query, CancellationToken cancellationToken = default);
        Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(SalesOrder salesOrder);
        void Update(SalesOrder salesOrder);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
