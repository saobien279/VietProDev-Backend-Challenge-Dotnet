using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Supplier : BaseEntity<Guid>
    {
        public string SupplierName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }

        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}