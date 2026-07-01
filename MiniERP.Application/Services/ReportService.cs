using System;
using System.IO;
using System.Text;
using System.Globalization;
using CsvHelper;
using System.Threading;
using System.Threading.Tasks;
using MiniERP.Application.DTOs.Reports;
using MiniERP.Application.DTOs.Common;
using MiniERP.Application.Exceptions;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                throw new BusinessValidationException("From date cannot be after to date.");
            }

            return await _reportRepository.GetRevenueReportAsync(fromDate, toDate, cancellationToken);
        }

        public async Task<PagedResult<LowStockProductDto>> GetLowStockReportAsync(int threshold, PaginationQuery query, CancellationToken cancellationToken = default)
        {
            if (threshold < 0)
            {
                throw new BusinessValidationException("Threshold cannot be negative.");
            }

            var (items, totalCount) = await _reportRepository.GetLowStockReportAsync(threshold, query, cancellationToken);

            return new PagedResult<LowStockProductDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                PageSize = query.Limit
            };
        }

        public async Task ExportSalesToCsvAsync(Stream targetStream, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                throw new BusinessValidationException("From date cannot be after to date.");
            }

            var itemsStream = _reportRepository.GetSalesOrderItemsStream(fromDate, toDate);

            using (var writer = new StreamWriter(targetStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
            {
                await writer.WriteLineAsync("sep=,");

                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Write Headers
                csv.WriteField("Sales Order ID");
                csv.WriteField("Customer Name");
                csv.WriteField("Order Date (UTC)");
                csv.WriteField("Status");
                csv.WriteField("Payment Status");
                csv.WriteField("SKU");
                csv.WriteField("Product Name");
                csv.WriteField("Quantity");
                csv.WriteField("Unit Price");
                csv.WriteField("Line Total");
                await csv.NextRecordAsync();

                await foreach (var item in itemsStream.WithCancellation(cancellationToken))
                {
                    csv.WriteField(item.SalesOrderId);
                    csv.WriteField(item.SalesOrder?.Customer?.CustomerName ?? "N/A");
                    csv.WriteField(item.SalesOrder?.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");
                    csv.WriteField(item.SalesOrder?.Status.ToString() ?? "N/A");
                    csv.WriteField(item.SalesOrder?.PaymentStatus.ToString() ?? "N/A");
                    csv.WriteField(item.Product?.Sku ?? "N/A");
                    csv.WriteField(item.Product?.ProductName ?? "N/A");
                    csv.WriteField(item.Quantity);
                    csv.WriteField(item.UnitPrice);
                    csv.WriteField(item.Quantity * item.UnitPrice);
                    await csv.NextRecordAsync();
                }

                await writer.FlushAsync(cancellationToken);
            }
        }
    }
}
}


