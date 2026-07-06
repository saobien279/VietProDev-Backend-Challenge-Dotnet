using System;

namespace MiniERP.Application.DTOs.Auth
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public System.Collections.Generic.List<string> Roles { get; set; } = new();
    }
}
