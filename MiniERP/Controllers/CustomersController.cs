using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Customers;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IValidator<CreateCustomerRequest> _createValidator;
        private readonly IValidator<UpdateCustomerRequest> _updateValidator;

        public CustomersController(
            ICustomerService customerService,
            IValidator<CreateCustomerRequest> createValidator,
            IValidator<UpdateCustomerRequest> updateValidator)
        {
            _customerService = customerService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
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
            var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    data = (object?)null,
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

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
            var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    data = (object?)null,
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Business validation failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Business validation failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
        }
    }
}
