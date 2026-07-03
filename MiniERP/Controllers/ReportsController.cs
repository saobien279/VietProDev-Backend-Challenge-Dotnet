using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Common;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var report = await _reportService.GetRevenueReportAsync(fromDate, toDate, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Revenue report generated successfully.",
                data = report,
                errors = (object?)null
            });
        }

        [HttpGet("inventory-low-stock")]
        public async Task<IActionResult> GetLowStock(
            [FromQuery] int threshold = 10,
            [FromQuery] PaginationQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            query ??= new PaginationQuery();
            var report = await _reportService.GetLowStockReportAsync(threshold, query, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Low stock report generated successfully.",
                data = report,
                errors = (object?)null
            });
        }

        [HttpGet("sales/export")]
        public async Task<IActionResult> ExportSales(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var fileName = $"sales_report_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            
            // Tạo file tạm để ghi dữ liệu, tránh OOM bằng cách ghi xuống đĩa thay vì RAM
            var tempFilePath = Path.GetTempFileName();
            
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await _reportService.ExportSalesToCsvAsync(fileStream, fromDate, toDate, cancellationToken);
            }

            // Mở file tạm để đọc và truyền cờ DeleteOnClose để hệ thống tự động xóa file sau khi truyền xong
            var readStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            
            return File(readStream, "text/csv", fileName);
        }
    }
}
