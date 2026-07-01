using System;
using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.SalesOrders
{
    public class SalesOrderItemDto : INormalizable
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public void Normalize()
        {
            // Nothing to normalize (no string fields)
        }
    }
}
