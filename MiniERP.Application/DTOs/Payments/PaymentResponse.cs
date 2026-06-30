using System;

namespace MiniERP.Application.DTOs.Payments
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid SalesOrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
