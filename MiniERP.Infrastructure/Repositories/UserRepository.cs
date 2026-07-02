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
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
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

        public async Task<System.Collections.Generic.List<string>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Select(r => r.RoleName)
                .ToListAsync(cancellationToken);
        }

        public async Task<System.Collections.Generic.List<Role>> GetRolesByNamesAsync(System.Collections.Generic.List<string> roleNames, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Where(r => roleNames.Contains(r.RoleName))
                .ToListAsync(cancellationToken);
        }

        public async Task ReplaceUserRolesAsync(Guid userId, System.Collections.Generic.List<int> newRoleIds, CancellationToken cancellationToken = default)
        {
            // Delete all existing roles for this user using ExecuteDeleteAsync
            // This bypasses the Change Tracker entirely, avoiding concurrency issues
            await _context.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            // Detach any tracked UserRole entities that were just deleted from DB
            foreach (var entry in _context.ChangeTracker.Entries<UserRole>().ToList())
            {
                if (entry.Entity.UserId == userId)
                    entry.State = EntityState.Detached;
            }

            // Add new roles
            foreach (var roleId in newRoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
