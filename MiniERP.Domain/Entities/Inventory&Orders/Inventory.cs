using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Inventory : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}