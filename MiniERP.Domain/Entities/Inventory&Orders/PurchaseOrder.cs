using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "DRAFT"; // DRAFT, CONFIRMED, CANCELLED
        public Guid CreatedBy { get; set; }
        public virtual User Creator { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}