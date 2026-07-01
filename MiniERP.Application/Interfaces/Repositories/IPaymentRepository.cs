using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetBySalesOrderIdAsync(Guid salesOrderId, CancellationToken cancellationToken = default);
        void Add(Payment payment);
    }
}
