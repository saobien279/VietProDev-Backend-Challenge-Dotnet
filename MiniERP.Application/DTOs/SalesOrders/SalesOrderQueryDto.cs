using System;
using MiniERP.Application.DTOs.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.DTOs.SalesOrders
{
    public class SalesOrderQueryDto : PaginationQuery
    {
        public SalesOrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
