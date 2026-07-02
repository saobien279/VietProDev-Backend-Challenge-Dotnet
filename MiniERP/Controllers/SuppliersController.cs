using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Suppliers;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF,ACCOUNTANT")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
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
        [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
        {
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
        [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
        {
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
        [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
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
