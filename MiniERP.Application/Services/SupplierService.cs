using MiniERP.Application.DTOs.Suppliers;
using MiniERP.Application.Exceptions;
using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<IEnumerable<SupplierResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var suppliers = await _supplierRepository.GetAllAsync(cancellationToken);
            return suppliers.Select(s => new SupplierResponse
            {
                Id = s.Id,
                SupplierName = s.SupplierName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<SupplierResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null) return null;

            return new SupplierResponse
            {
                Id = supplier.Id,
                SupplierName = supplier.SupplierName,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                CreatedAt = supplier.CreatedAt
            };
        }

        public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
        {
            var supplier = new Supplier
            {
                SupplierName = request.SupplierName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address
            };

            _supplierRepository.Add(supplier);
            await _supplierRepository.SaveChangesAsync(cancellationToken);

            return new SupplierResponse
            {
                Id = supplier.Id,
                SupplierName = supplier.SupplierName,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                CreatedAt = supplier.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null) return false;

            var phoneExists = await _supplierRepository.AnyAsync(s => s.Phone == request.Phone && s.Id != id, cancellationToken);
            if (phoneExists)
            {
                throw new BusinessValidationException("Phone number already belongs to another supplier.");
            }

            supplier.SupplierName = request.SupplierName;
            supplier.Email = request.Email;
            supplier.Phone = request.Phone;
            supplier.Address = request.Address;

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken);
            if (supplier == null) return false;

            var hasOrders = await _supplierRepository.HasPurchaseOrdersAsync(id, cancellationToken);
            if (hasOrders)
            {
                throw new BusinessValidationException("Cannot delete supplier because they have associated purchase orders.");
            }

            _supplierRepository.Delete(supplier);
            await _supplierRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
