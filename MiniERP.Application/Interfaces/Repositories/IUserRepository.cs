using MiniERP.Domain.Entities;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(User user);
        Task<System.Collections.Generic.List<string>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<System.Collections.Generic.List<Role>> GetRolesByNamesAsync(System.Collections.Generic.List<string> roleNames, CancellationToken cancellationToken = default);
        Task ReplaceUserRolesAsync(Guid userId, System.Collections.Generic.List<int> newRoleIds, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
