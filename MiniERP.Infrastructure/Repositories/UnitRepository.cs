 using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System.Linq.Expressions;

namespace MiniERP.Infrastructure.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Units.ToListAsync(cancellationToken);
        }

        public async Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Units.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<Unit, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Units.AnyAsync(predicate, cancellationToken);
        }

        public void Add(Unit unit)
        {
            _context.Units.Add(unit);
        }

        public void Update(Unit unit)
        {
            _context.Units.Update(unit);
        }

        public void Delete(Unit unit)
        {
            _context.Units.Remove(unit); // DbContext intercepts → Soft Delete
        }

        public async Task<bool> HasProductsAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.UnitId == unitId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
