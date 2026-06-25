using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Suppliers
{
    public class UpdateSupplierRequest : INormalizable
    {
        public string SupplierName { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }

        public void Normalize()
        {
            SupplierName = string.IsNullOrWhiteSpace(SupplierName) 
                ? string.Empty 
                : Regex.Replace(SupplierName.Trim(), @"\s+", " ");
                
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