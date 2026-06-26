using System;
using System.Collections.Generic;

using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public class StockTransaction : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
        public Guid? ReferenceId { get; set; } // Có thể là Id của SalesOrder hoặc PurchaseOrder
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public ReferenceType? ReferenceType { get; set; }
    }
}