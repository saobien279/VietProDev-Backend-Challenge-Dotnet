namespace MiniERP.Application.DTOs.Customers
{
    public class UpdateCustomerRequest
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
    }
}