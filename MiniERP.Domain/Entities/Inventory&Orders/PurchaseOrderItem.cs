using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class PurchaseOrderItem : BaseEntity<Guid>
    {
        public Guid PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}