namespace MiniERP.Application.DTOs.Products
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
