using MiniERP.Application.DTOs.Auth;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<UserProfileResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<System.Collections.Generic.List<string>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task AssignRolesAsync(Guid userId, System.Collections.Generic.List<string> roles, CancellationToken cancellationToken = default);
    }
}
