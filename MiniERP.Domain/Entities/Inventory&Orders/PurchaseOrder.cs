using System;
using System.Collections.Generic;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public class PurchaseOrder : BaseEntity<Guid>
    {
        public Guid SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.DRAFT;
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}