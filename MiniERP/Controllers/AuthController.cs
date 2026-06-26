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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(
            IAuthService authService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var profile = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "User registered successfully.",
                data = profile,
                errors = (object?)null
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var authResponse = await _authService.LoginAsync(request, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Login successful.",
                data = authResponse,
                errors = (object?)null
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Unauthorized access.",
                    data = (object?)null,
                    errors = new[] { "User ID not found in claims." }
                });
            }

            var profile = await _authService.GetProfileAsync(userId.Value, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved user profile successfully.",
                data = profile,
                errors = (object?)null
            });
        }
    }
}
