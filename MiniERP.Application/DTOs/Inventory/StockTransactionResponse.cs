using System;

namespace MiniERP.Application.DTOs.Inventory
{
    public class StockTransactionResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string TransactionType { get; set; } = null!;
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
        public Guid? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
