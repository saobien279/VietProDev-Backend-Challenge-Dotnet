using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class SalesOrderItem : BaseEntity<Guid>
    {
        public Guid SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}