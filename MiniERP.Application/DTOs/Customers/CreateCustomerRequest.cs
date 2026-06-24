using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Customers
{
    public class CreateCustomerRequest : INormalizable
    {
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }

        public void Normalize()
        {
            CustomerName = string.IsNullOrWhiteSpace(CustomerName) 
                ? string.Empty 
                : Regex.Replace(CustomerName.Trim(), @"\s+", " ");
                
            Email = string.IsNullOrWhiteSpace(Email) 
                ? null 
                : Email.Trim().ToLower();
                
            Phone = string.IsNullOrWhiteSpace(Phone) 
                ? string.Empty 
                : Regex.Replace(Phone, @"\D", "");
                
            Address = string.IsNullOrWhiteSpace(Address) 
                ? null 
                : Regex.Replace(Address, @"\s", "");
        }
    }
}