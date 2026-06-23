namespace MiniERP.Application.DTOs.Customers
{
    public class CreateCustomerRequest
    {
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
    }
}