using System;

namespace MiniERP.Application.DTOs.Reports
{
    public class LowStockProductDto
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int CurrentQuantity { get; set; }
    }
}
