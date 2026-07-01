using MiniERP.Domain.Enums;
using MiniERP.Application.Interfaces;

namespace MiniERP.Application.DTOs.Payments
{
    public class CreatePaymentRequest : INormalizable
    {
        public decimal PaymentAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public void Normalize()
        {

        }
    }
}
