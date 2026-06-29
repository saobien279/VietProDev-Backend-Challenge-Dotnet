using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System;
using System.Collections.Generic;
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

        public async Task<IEnumerable<SalesOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SalesOrders
                .Include(so => so.Customer)
                .ToListAsync(cancellationToken);
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
