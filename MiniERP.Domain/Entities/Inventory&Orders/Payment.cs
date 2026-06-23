using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Payment : BaseEntity<Guid>
    {
        public Guid SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = null!; // CASH, BANK_TRANSFER
    }
}