using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System.Linq.Expressions;

namespace MiniERP.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers.ToListAsync(cancellationToken);
        }

        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.AnyAsync(predicate, cancellationToken);
        }

        public void Add(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public void Update(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public void Delete(Customer customer)
        {
            _context.Customers.Remove(customer); // Sử dụng Remove, DbContext sẽ tự động chuyển đổi sang Soft Delete ở SaveChanges
        }

        public async Task<bool> HasSalesOrdersAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.SalesOrders.AnyAsync(o => o.CustomerId == customerId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}