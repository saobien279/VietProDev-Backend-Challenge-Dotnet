namespace MiniERP.Application.DTOs.Suppliers
{
    public class UpdateSupplierRequest
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
    }
}