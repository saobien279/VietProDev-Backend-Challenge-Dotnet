using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniERP.Application.DTOs.Auth;
using MiniERP.Application.Exceptions;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> _logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            this._logger = _logger;
        }

        public async Task<UserProfileResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var usernameExists = await _userRepository.AnyAsync(u => u.Username == request.Username, cancellationToken);
            if (usernameExists)
            {
                throw new DuplicateResourceException("Username already exists.");
            }

            var emailExists = await _userRepository.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailExists)
            {
                throw new DuplicateResourceException("Email already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                IsActive = true
            };

            _userRepository.Add(user);
            
            try
            {
                await _userRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Database update exception occurred when registering user {Username} / {Email}.", request.Username, request.Email);
                throw new DuplicateResourceException("Username or email already exists.");
            }

            return new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt: Email {Email} does not exist.", request.Email);
                throw new BusinessValidationException("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt: Incorrect password for email {Email}.", request.Email);
                throw new BusinessValidationException("Invalid email or password.");
            }

            if (user.DeletedAt != null)
            {
                _logger.LogWarning("Failed login attempt: Account for email {Email} is soft deleted.", request.Email);
                throw new NotFoundException("User not found.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Failed login attempt: Account for email {Email} is deactivated.", request.Email);
                throw new AccountDeactivatedException("Account is deactivated.");
            }

            var authResponse = _jwtService.GenerateToken(user);
            return authResponse;
        }

        public async Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || user.DeletedAt != null)
            {
                throw new NotFoundException("User not found.");
            }

            if (!user.IsActive)
            {
                throw new AccountDeactivatedException("Account is deactivated.");
            }

            return new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
