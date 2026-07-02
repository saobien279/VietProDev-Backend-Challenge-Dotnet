using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using System.Net;
using System.Threading.Tasks;

namespace MiniERP.Filters
{
    public class CheckUserStatusFilter : IAsyncActionFilter
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public CheckUserStatusFilter(ICurrentUserService currentUserService, IUserRepository userRepository)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = _currentUserService.UserId;
            if (userId.HasValue)
            {
                var isUserActive = await _userRepository.AnyAsync(u => u.Id == userId.Value && u.IsActive && u.DeletedAt == null );
                if (!isUserActive)
                {
                    context.Result = new ObjectResult(new
                    {
                        success = false,
                        message = "Your account has been deactivated or deleted. Access denied.",
                        data = (object?)null,
                        errors = new[] { "User account is inactive or deleted." }
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden
                    };
                    return; // Short-circuit the request
                }
            }

            await next();
        }
    }
}
