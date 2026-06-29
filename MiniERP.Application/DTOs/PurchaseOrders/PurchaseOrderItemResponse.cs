using System;

namespace MiniERP.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
