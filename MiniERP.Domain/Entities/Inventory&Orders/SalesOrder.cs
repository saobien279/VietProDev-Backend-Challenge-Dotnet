using System;
using System.Collections.Generic;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public class SalesOrder : BaseEntity<Guid>
    {
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.DRAFT;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.UNPAID;
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}