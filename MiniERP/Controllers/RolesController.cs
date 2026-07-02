using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class RolesController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RolesController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            var roles = await _authService.GetRolesAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Roles retrieved successfully.",
                data = roles,
                errors = (object?)null
            });
        }
    }
}
