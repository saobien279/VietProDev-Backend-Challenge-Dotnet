using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Auth;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Linq;
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
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthController(
            IAuthService authService,
            ICurrentUserService currentUserService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    data = (object?)null,
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Registration failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    data = (object?)null,
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Login failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Login failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
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
            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User profile not found.",
                    data = (object?)null,
                    errors = new[] { "User does not exist or has been deleted." }
                });
            }

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
