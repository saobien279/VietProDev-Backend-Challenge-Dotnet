using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Suppliers;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IValidator<CreateSupplierRequest> _createValidator;
        private readonly IValidator<UpdateSupplierRequest> _updateValidator;

        public SuppliersController(
            ISupplierService supplierService,
            IValidator<CreateSupplierRequest> createValidator,
            IValidator<UpdateSupplierRequest> updateValidator)
        {
            _supplierService = supplierService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var suppliers = await _supplierService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all suppliers successfully.",
                data = suppliers,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Supplier with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Supplier with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved supplier successfully.",
                data = supplier,
                errors = (object?)null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
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

            var createdSupplier = await _supplierService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdSupplier.Id }, new
            {
                success = true,
                message = "Supplier created successfully.",
                data = createdSupplier,
                errors = (object?)null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID mismatch.",
                    data = (object?)null,
                    errors = new[] { "The ID in the URL route must match the ID in the request body." }
                });
            }

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

            var updated = await _supplierService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Supplier with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Supplier with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Supplier updated successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _supplierService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Supplier with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Supplier with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Supplier deleted successfully (soft delete).",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
