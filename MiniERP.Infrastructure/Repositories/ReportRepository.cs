using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.Common;
using MiniERP.Application.DTOs.Reports;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Domain.Enums;
using MiniERP.Domain.Entities;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        {
            var query = _context.SalesOrders
                .AsNoTracking()
                .Where(so => so.Status == SalesOrderStatus.CONFIRMED);

            if (fromDate.HasValue)
            {
                query = query.Where(so => so.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(so => so.CreatedAt <= toDate.Value);
            }

            var totalRevenue = await query.SumAsync(so => (decimal?)so.TotalAmount, cancellationToken) ?? 0;
            var totalOrders = await query.CountAsync(cancellationToken);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var dailyBreakdown = await query
                .GroupBy(so => so.CreatedAt.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(so => so.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);

            return new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                FromDate = fromDate,
                ToDate = toDate,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<(IEnumerable<LowStockProductDto> Items, int TotalCount)> GetLowStockReportAsync(int threshold, PaginationQuery query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Products
                .AsNoTracking()
                .Where(p => p.Inventory == null || p.Inventory.Quantity <= threshold);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchPattern = $"%{query.Search}%";
                queryable = queryable.Where(p => EF.Functions.ILike(p.ProductName, searchPattern) || EF.Functions.ILike(p.Sku, searchPattern));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            // Sorting
            queryable = query.SortBy?.ToLower() switch
            {
                "sku" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(p => p.Sku) : queryable.OrderBy(p => p.Sku),
                "productname" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(p => p.ProductName) : queryable.OrderBy(p => p.ProductName),
                "quantity" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(p => p.Inventory != null ? p.Inventory.Quantity : 0) : queryable.OrderBy(p => p.Inventory != null ? p.Inventory.Quantity : 0),
                _ => queryable.OrderBy(p => p.Inventory != null ? p.Inventory.Quantity : 0)
            };

            var items = await queryable
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    CurrentQuantity = p.Inventory != null ? p.Inventory.Quantity : 0
                })
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public IAsyncEnumerable<SalesOrderItem> GetSalesOrderItemsStream(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.SalesOrderItems
                .AsNoTracking()
                .Include(soi => soi.SalesOrder)
                    .ThenInclude(so => so.Customer)
                .Include(soi => soi.Product)
                .Where(soi => soi.SalesOrder.DeletedAt == null);

            if (fromDate.HasValue)
            {
                query = query.Where(soi => soi.SalesOrder.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(soi => soi.SalesOrder.CreatedAt <= toDate.Value);
            }

            return query
                .OrderByDescending(soi => soi.SalesOrder.CreatedAt)
                .AsAsyncEnumerable();
        }
    }
}
