using System;
using System.Collections.Generic;

namespace MiniERP.Application.DTOs.Reports
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new List<DailyRevenueDto>();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
