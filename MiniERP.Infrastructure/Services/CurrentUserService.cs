using Microsoft.AspNetCore.Http;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Security.Claims;

namespace MiniERP.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

                if (Guid.TryParse(userIdClaim, out var parsedGuid))
                {
                    return parsedGuid;
                }
                return null;
            }
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public bool IsAdmin => IsInRole("ADMIN");
        public bool IsManager => IsInRole("MANAGER");
    }
}
