using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Auth
{
    public class RegisterRequest : INormalizable
    {
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public void Normalize()
        {
            Username = string.IsNullOrWhiteSpace(Username) ? string.Empty : Username.Trim();
            FullName = string.IsNullOrWhiteSpace(FullName) ? string.Empty : Regex.Replace(FullName.Trim(), @"\s+", " ");
            Email = string.IsNullOrWhiteSpace(Email) ? string.Empty : Email.Trim().ToLower();
            // Password is intentionally not normalized to preserve password characters.
        }
    }
}
