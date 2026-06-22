using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}