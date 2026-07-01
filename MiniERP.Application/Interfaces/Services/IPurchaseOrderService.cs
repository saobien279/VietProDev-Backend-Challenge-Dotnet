using MiniERP.Application.DTOs.PurchaseOrders;
using MiniERP.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<PurchaseOrderResponse>> GetPagedAsync(PurchaseOrderQueryDto query, CancellationToken cancellationToken = default);
        Task<PurchaseOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
        Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
        Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
