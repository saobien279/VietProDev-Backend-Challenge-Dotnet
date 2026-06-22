using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}