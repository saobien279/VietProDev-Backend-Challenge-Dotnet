using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class SalesOrder
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "DRAFT"; // DRAFT, CONFIRMED, CANCELLED
        public string PaymentStatus { get; set; } = "UNPAID"; // UNPAID, PARTIAL, PAID
        public Guid CreatedBy { get; set; }
        public virtual User Creator { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}