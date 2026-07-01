using System;

namespace MiniERP.Domain.Entities
{
    public class DailySummary : BaseEntity<Guid>
    {
        public DateTime SummaryDate { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public int TotalSalesOrdersCount { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public int TotalPurchaseOrdersCount { get; set; }
        public int LowStockProductsCount { get; set; }
    }
}
