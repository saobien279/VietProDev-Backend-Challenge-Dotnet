using MiniERP.Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentResponse>> GetPaymentsBySalesOrderIdAsync(Guid salesOrderId, CancellationToken cancellationToken = default);
        Task<PaymentResponse> AddPaymentAsync(Guid salesOrderId, CreatePaymentRequest request, CancellationToken cancellationToken = default);
    }
}
