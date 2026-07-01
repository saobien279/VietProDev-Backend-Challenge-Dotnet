using System;
using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderItemDto : INormalizable
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public void Normalize()
        {
            // Nothing to normalize (no string fields)
        }
    }
}
