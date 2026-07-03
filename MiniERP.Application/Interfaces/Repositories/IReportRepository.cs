using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniERP.Application.DTOs.Reports;
using MiniERP.Application.DTOs.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
        Task<(IEnumerable<LowStockProductDto> Items, int TotalCount)> GetLowStockReportAsync(int threshold, PaginationQuery query, CancellationToken cancellationToken = default);
        IAsyncEnumerable<SalesOrderItem> GetSalesOrderItemsStream(DateTime? fromDate, DateTime? toDate);
    }
}
