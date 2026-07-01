using System;
using System.Collections.Generic;

namespace MiniERP.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderResponse
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public List<PurchaseOrderItemResponse> Items { get; set; } = new();
    }
}
