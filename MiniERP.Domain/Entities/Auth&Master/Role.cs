using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!; // ADMIN, STAFF, ACCOUNTANT, MANAGER

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}