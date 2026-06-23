using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Customer : BaseEntity<Guid>
    {
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }

        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}