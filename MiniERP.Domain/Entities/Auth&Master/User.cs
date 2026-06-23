using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class User : BaseEntity<Guid>
    {
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<SalesOrder> CreatedSalesOrders { get; set; } = new List<SalesOrder>();
        public virtual ICollection<PurchaseOrder> CreatedPurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}