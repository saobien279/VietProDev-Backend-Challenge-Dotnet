using System;
using System.Collections.Generic;

namespace MiniERP.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; } = null!;

        // 1-1 với Inventory
        public virtual Inventory? Inventory { get; set; }
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}