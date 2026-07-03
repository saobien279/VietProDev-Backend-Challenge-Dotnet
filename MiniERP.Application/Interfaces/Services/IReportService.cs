using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MiniERP.Application.DTOs.Reports;
using MiniERP.Application.DTOs.Common;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
        Task<PagedResult<LowStockProductDto>> GetLowStockReportAsync(int threshold, PaginationQuery query, CancellationToken cancellationToken = default);
        Task ExportSalesToCsvAsync(Stream targetStream, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    }
}
