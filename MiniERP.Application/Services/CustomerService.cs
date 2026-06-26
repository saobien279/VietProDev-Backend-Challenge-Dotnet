using MiniERP.Application.DTOs.Customers;
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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var customers = await _customerRepository.GetAllAsync(cancellationToken);
            return customers.Select(c => new CustomerResponse
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
            if (customer == null) return null;

            return new CustomerResponse
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            var customer = new Customer
            {
                CustomerName = request.CustomerName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address
            };

            _customerRepository.Add(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return new CustomerResponse
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
            if (customer == null) return false;

            var phoneExists = await _customerRepository.AnyAsync(c => c.Phone == request.Phone && c.Id != id, cancellationToken);
            if (phoneExists)
            {
                throw new BusinessValidationException("Phone number already belongs to another customer.");
            }

            customer.CustomerName = request.CustomerName;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.Address = request.Address;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
            if (customer == null) return false;

            var hasOrders = await _customerRepository.HasSalesOrdersAsync(id, cancellationToken);
            if (hasOrders)
            {
                throw new BusinessValidationException("Cannot delete customer because they have associated sales orders.");
            }

            _customerRepository.Delete(customer);
            await _customerRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
