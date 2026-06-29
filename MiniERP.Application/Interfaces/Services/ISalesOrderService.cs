using MiniERP.Application.DTOs.SalesOrders;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrderResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SalesOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SalesOrderResponse> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default);
        Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
        Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
