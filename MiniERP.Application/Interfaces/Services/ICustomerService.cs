using MiniERP.Application.DTOs.Customers;
using MiniERP.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<CustomerResponse>> GetPagedAsync(CustomerQueryDto query, CancellationToken cancellationToken = default);
        Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
