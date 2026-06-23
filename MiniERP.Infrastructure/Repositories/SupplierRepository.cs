using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.ToListAsync(cancellationToken);
        }

        public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<Supplier, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(predicate, cancellationToken);
        }

        public void Add(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
        }

        public void Update(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
        }

        public void Delete(Supplier supplier)
        {
            _context.Suppliers.Update(supplier); // Soft Delete
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
