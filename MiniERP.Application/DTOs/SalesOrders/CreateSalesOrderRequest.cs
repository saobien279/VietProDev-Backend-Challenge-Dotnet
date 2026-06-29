using System;
using System.Collections.Generic;
using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.SalesOrders
{
    public class CreateSalesOrderRequest : INormalizable
    {
        public Guid CustomerId { get; set; }
        public List<SalesOrderItemDto> Items { get; set; } = new();

        public void Normalize()
        {
            // Nothing to normalize (no string fields)
        }
    }
}
