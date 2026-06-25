using MiniERP.Application.DTOs.Auth;
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

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<UserProfileResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var usernameExists = await _userRepository.AnyAsync(u => u.Username == request.Username, cancellationToken);
            if (usernameExists)
            {
                throw new ArgumentException("Username already exists.");
            }

            var emailExists = await _userRepository.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailExists)
            {
                throw new ArgumentException("Email already exists.");
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
            await _userRepository.SaveChangesAsync(cancellationToken);

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
                throw new ArgumentException("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ArgumentException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException("Account is deactivated.");
            }

            var authResponse = _jwtService.GenerateToken(user);
            return authResponse;
        }

        public async Task<UserProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return null;

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
