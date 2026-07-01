using MiniERP.Domain.Entities;
using MiniERP.Application.DTOs.Auth;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IJwtService
    {
        AuthResponse GenerateToken(User user);
    }
}
