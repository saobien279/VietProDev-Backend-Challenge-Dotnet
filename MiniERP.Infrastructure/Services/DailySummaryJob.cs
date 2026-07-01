using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services
{
    public class DailySummaryJob : IDailySummaryJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DailySummaryJob> _logger;

        public DailySummaryJob(ApplicationDbContext context, ILogger<DailySummaryJob> _logger)
        {
            _context = context;
            this._logger = _logger;
        }

        public async Task ExecuteAsync(DateTime? targetDate = null)
        {
            TimeZoneInfo vietnamTimeZone;
            try
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var date = targetDate?.Date ?? localTime.AddDays(-1).Date;

            // Tính toán thời gian bắt đầu và kết thúc ngày theo giờ Việt Nam
            var startLocal = date;
            var endLocal = startLocal.AddDays(1).AddTicks(-1);

            // Chuyển đổi sang UTC để query DB chính xác (do database lưu CreatedAt theo UTC)
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, vietnamTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, vietnamTimeZone);

            // Ngày lưu vào DB ở dạng 00:00:00 UTC làm mốc calendar date thống nhất
            var summaryDateDb = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            try
            {
                _logger.LogInformation("Starting Daily Summary Job for date: {Date:yyyy-MM-dd} (VN Time) / UTC Range: {StartUtc:O} - {EndUtc:O}", date, startUtc, endUtc);

                // 1. Calculate sales metrics
                var totalSalesRevenue = await _context.SalesOrders
                    .Where(so => so.Status == SalesOrderStatus.CONFIRMED && so.CreatedAt >= startUtc && so.CreatedAt <= endUtc)
                    .SumAsync(so => (decimal?)so.TotalAmount) ?? 0;

                var totalSalesOrdersCount = await _context.SalesOrders
                    .Where(so => so.Status == SalesOrderStatus.CONFIRMED && so.CreatedAt >= startUtc && so.CreatedAt <= endUtc)
                    .CountAsync();

                // 2. Calculate purchase metrics
                var totalPurchaseCost = await _context.PurchaseOrders
                    .Where(po => po.Status == PurchaseOrderStatus.CONFIRMED && po.CreatedAt >= startUtc && po.CreatedAt <= endUtc)
                    .SumAsync(po => (decimal?)po.TotalAmount) ?? 0;

                var totalPurchaseOrdersCount = await _context.PurchaseOrders
                    .Where(po => po.Status == PurchaseOrderStatus.CONFIRMED && po.CreatedAt >= startUtc && po.CreatedAt <= endUtc)
                    .CountAsync();

                // 3. Low stock count
                var lowStockProductsCount = await _context.Products
                    .CountAsync(p => p.Inventory == null || p.Inventory.Quantity <= 10);

                // 4. Upsert summary
                var existingSummary = await _context.DailySummaries
                    .FirstOrDefaultAsync(ds => ds.SummaryDate == summaryDateDb);

                if (existingSummary != null)
                {
                    existingSummary.TotalSalesRevenue = totalSalesRevenue;
                    existingSummary.TotalSalesOrdersCount = totalSalesOrdersCount;
                    existingSummary.TotalPurchaseCost = totalPurchaseCost;
                    existingSummary.TotalPurchaseOrdersCount = totalPurchaseOrdersCount;
                    existingSummary.LowStockProductsCount = lowStockProductsCount;
                    existingSummary.UpdatedAt = DateTime.UtcNow;

                    _context.DailySummaries.Update(existingSummary);
                }
                else
                {
                    var newSummary = new DailySummary
                    {
                        SummaryDate = summaryDateDb,
                        TotalSalesRevenue = totalSalesRevenue,
                        TotalSalesOrdersCount = totalSalesOrdersCount,
                        TotalPurchaseCost = totalPurchaseCost,
                        TotalPurchaseOrdersCount = totalPurchaseOrdersCount,
                        LowStockProductsCount = lowStockProductsCount
                    };

                    await _context.DailySummaries.AddAsync(newSummary);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully completed Daily Summary Job for date: {Date:yyyy-MM-dd}. Sales Revenue: {Rev}, Sales Count: {SC}, Purchase Cost: {Cost}, Purchase Count: {PC}, Low Stock: {LS}",
                    date, totalSalesRevenue, totalSalesOrdersCount, totalPurchaseCost, totalPurchaseOrdersCount, lowStockProductsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Daily Summary Job for date: {Date:yyyy-MM-dd}", date);
                throw;
            }
        }
    }
}
