using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Supplier
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}