using System;
using System.Collections.Generic;
using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.PurchaseOrders
{
    public class CreatePurchaseOrderRequest : INormalizable
    {
        public Guid SupplierId { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new();

        public void Normalize()
        {
            // Nothing to normalize (no string fields)
        }
    }
}
