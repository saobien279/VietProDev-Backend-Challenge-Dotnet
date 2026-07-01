using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Products
{
    public class CreateProductRequest : INormalizable
    {
        public string Sku { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CategoryId { get; set; }
        public int UnitId { get; set; }
        public bool IsActive { get; set; } = true;

        public void Normalize()
        {
            Sku = string.IsNullOrWhiteSpace(Sku)
                ? string.Empty
                : Sku.Trim().ToUpperInvariant();

            ProductName = string.IsNullOrWhiteSpace(ProductName)
                ? string.Empty
                : Regex.Replace(ProductName.Trim(), @"\s+", " ");
        }
    }
}
