using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Inventory;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] ImportStockRequest request, CancellationToken cancellationToken)
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

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportStockRequest request, CancellationToken cancellationToken)
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
