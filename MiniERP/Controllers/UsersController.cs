using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Auth;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRoles(
            Guid id,
            [FromBody] AssignRolesRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.AssignRolesAsync(id, request.Roles, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Roles assigned successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
