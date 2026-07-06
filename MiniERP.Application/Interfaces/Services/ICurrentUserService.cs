using System;

namespace MiniERP.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        bool IsInRole(string role);
        bool IsAdmin { get; }
        bool IsManager { get; }
    }
}
