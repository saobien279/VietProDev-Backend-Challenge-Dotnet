using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Infrastructure.Data;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(predicate, cancellationToken);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
