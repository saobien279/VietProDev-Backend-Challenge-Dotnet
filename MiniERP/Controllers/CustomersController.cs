using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Customers;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var customers = await _customerService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all customers successfully.",
                data = customers,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var customer = await _customerService.GetByIdAsync(id, cancellationToken);
            if (customer == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Customer with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Customer with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved customer successfully.",
                data = customer,
                errors = (object?)null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var createdCustomer = await _customerService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, new
            {
                success = true,
                message = "Customer created successfully.",
                data = createdCustomer,
                errors = (object?)null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var updated = await _customerService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Customer with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Customer with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Customer updated successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _customerService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Customer with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Customer with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Customer deleted successfully (soft delete).",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
