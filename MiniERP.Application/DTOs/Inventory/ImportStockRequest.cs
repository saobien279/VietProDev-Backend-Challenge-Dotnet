using MiniERP.Application.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Inventory
{
    public class ImportStockRequest : INormalizable
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;

        public void Normalize()
        {
            Reason = string.IsNullOrWhiteSpace(Reason)
                ? string.Empty
                : Regex.Replace(Reason.Trim(), @"\s+", " ");
        }
    }
}
