using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.Auth
{
    public class LoginRequest : INormalizable
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public void Normalize()
        {
            Email = string.IsNullOrWhiteSpace(Email) ? string.Empty : Email.Trim().ToLower();
            // Password is intentionally not normalized.
        }
    }
}
