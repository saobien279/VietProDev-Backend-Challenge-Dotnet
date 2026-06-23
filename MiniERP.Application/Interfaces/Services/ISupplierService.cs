using MiniERP.Application.DTOs.Suppliers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SupplierResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
