using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs.SalesOrders;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Domain.Entities;
using MiniERP.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllAsync(Guid? createdByFilter = null, CancellationToken cancellationToken = default)
        {
            var query = _context.SalesOrders.AsQueryable();
            if (createdByFilter.HasValue)
            {
                query = query.Where(so => so.CreatedBy == createdByFilter.Value);
            }
            return await query
                .Include(so => so.Customer)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<SalesOrder> Items, int TotalCount)> GetPagedAsync(SalesOrderQueryDto query, CancellationToken cancellationToken = default)
        {
            var queryable = _context.SalesOrders.AsQueryable();

            if (query.CreatedByFilter.HasValue)
            {
                queryable = queryable.Where(so => so.CreatedBy == query.CreatedByFilter.Value);
            }

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(so => so.Status == query.Status.Value);
            }

            if (query.FromDate.HasValue)
            {
                queryable = queryable.Where(so => so.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                queryable = queryable.Where(so => so.CreatedAt <= query.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchPattern = $"%{query.Search}%";
                queryable = queryable.Where(so => EF.Functions.ILike(so.Customer.CustomerName, searchPattern));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            queryable = query.SortBy?.ToLower() switch
            {
                "totalamount" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(so => so.TotalAmount) : queryable.OrderBy(so => so.TotalAmount),
                "status" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(so => so.Status) : queryable.OrderBy(so => so.Status),
                "createdat" => query.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(so => so.CreatedAt) : queryable.OrderBy(so => so.CreatedAt),
                _ => queryable.OrderByDescending(so => so.CreatedAt)
            };

            var items = await queryable
                .Include(so => so.Customer)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                    .ThenInclude(soi => soi.Product)
                .FirstOrDefaultAsync(so => so.Id == id, cancellationToken);
        }

        public void Add(SalesOrder salesOrder)
        {
            _context.SalesOrders.Add(salesOrder);
        }

        public void Update(SalesOrder salesOrder)
        {
            _context.SalesOrders.Update(salesOrder);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
