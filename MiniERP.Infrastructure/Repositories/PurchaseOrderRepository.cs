using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.PurchaseOrders;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace MiniERP.Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(PurchaseOrderQueryDto query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.PurchaseOrders.AsQueryable();

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(po => po.Status == query.Status.Value);
            }

            if (query.FromDate.HasValue)
            {
                queryable = queryable.Where(po => po.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                queryable = queryable.Where(po => po.CreatedAt <= query.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchPattern = $"%{query.Search}%";
                queryable = queryable.Where(po => EF.Functions.ILike(po.Supplier.SupplierName, searchPattern));
            }


            var totalCount = await queryable.CountAsync(cancellationToken);

            queryable = query.SortBy?.ToLower() switch
            {
                "totalamount" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(po => po.TotalAmount) : queryable.OrderBy(po => po.TotalAmount),
                "status" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(po => po.Status) : queryable.OrderBy(po => po.Status),
                "createdat" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(po => po.CreatedAt) : queryable.OrderBy(po => po.CreatedAt),
                _ => queryable.OrderByDescending(po => po.CreatedAt)
            };

            var items = await queryable
                .Include(po => po.Supplier)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
        }

        public void Add(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Add(purchaseOrder);
        }

        public void Update(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Update(purchaseOrder);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
