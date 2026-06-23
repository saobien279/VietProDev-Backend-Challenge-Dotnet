using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class SalesOrder : BaseEntity<Guid>
    {
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "DRAFT"; // DRAFT, CONFIRMED, CANCELLED
        public string PaymentStatus { get; set; } = "UNPAID"; // UNPAID, PARTIAL, PAID
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}