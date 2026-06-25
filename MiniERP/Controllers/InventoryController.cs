using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Inventory;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IValidator<ImportStockRequest> _importValidator;
        private readonly IValidator<ExportStockRequest> _exportValidator;

        public InventoryController(
            IInventoryService inventoryService,
            IValidator<ImportStockRequest> importValidator,
            IValidator<ExportStockRequest> exportValidator)
        {
            _inventoryService = inventoryService;
            _importValidator = importValidator;
            _exportValidator = exportValidator;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] ImportStockRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _importValidator.ValidateAsync(request, cancellationToken);
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
                var result = await _inventoryService.ImportAsync(request, cancellationToken);
                return Ok(new
                {
                    success = true,
                    message = "Stock imported successfully.",
                    data = result,
                    errors = (object?)null
                });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Concurrent"))
                {
                    return StatusCode(409, new
                    {
                        success = false,
                        message = "Concurrency conflict occurred.",
                        data = (object?)null,
                        errors = new[] { ex.Message }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Business validation failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportStockRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _exportValidator.ValidateAsync(request, cancellationToken);
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
                var result = await _inventoryService.ExportAsync(request, cancellationToken);
                return Ok(new
                {
                    success = true,
                    message = "Stock exported successfully.",
                    data = result,
                    errors = (object?)null
                });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Concurrent"))
                {
                    return StatusCode(409, new
                    {
                        success = false,
                        message = "Concurrency conflict occurred.",
                        data = (object?)null,
                        errors = new[] { ex.Message }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Business validation failed.",
                    data = (object?)null,
                    errors = new[] { ex.Message }
                });
            }
        }

        [HttpGet("products/{productId:guid}")]
        public async Task<IActionResult> GetStockByProductId(Guid productId, CancellationToken cancellationToken)
        {
            var result = await _inventoryService.GetStockByProductIdAsync(productId, cancellationToken);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Inventory not found.",
                    data = (object?)null,
                    errors = new[] { "Inventory for the specified product does not exist." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved inventory successfully.",
                data = result,
                errors = (object?)null
            });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(CancellationToken cancellationToken)
        {
            var result = await _inventoryService.GetTransactionsAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved stock transactions successfully.",
                data = result,
                errors = (object?)null
            });
        }
    }
}
