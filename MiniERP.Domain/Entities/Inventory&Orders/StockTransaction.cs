using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class StockTransaction : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string TransactionType { get; set; } = null!; // IMPORT, EXPORT
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
        public Guid? ReferenceId { get; set; } // Có thể là Id của SalesOrder hoặc PurchaseOrder
    }
}