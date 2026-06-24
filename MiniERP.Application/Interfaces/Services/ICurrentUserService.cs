using System;

namespace MiniERP.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
    }
}
