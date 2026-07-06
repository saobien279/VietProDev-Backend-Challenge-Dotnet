using System;
using MiniERP.Application.DTOs.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderQueryDto : PaginationQuery
    {
        public PurchaseOrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CreatedByFilter { get; set; }
    }
}
