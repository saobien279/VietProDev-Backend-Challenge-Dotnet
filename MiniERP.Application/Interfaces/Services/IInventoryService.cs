using MiniERP.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<InventoryResponse?> GetStockByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockTransactionResponse>> GetTransactionsAsync(CancellationToken cancellationToken = default);
        Task<InventoryResponse> ImportAsync(ImportStockRequest request, CancellationToken cancellationToken = default);
        Task<InventoryResponse> ExportAsync(ExportStockRequest request, CancellationToken cancellationToken = default);
    }
}
