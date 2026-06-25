using System;

namespace MiniERP.Application.DTOs.Inventory
{
    public class InventoryResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
