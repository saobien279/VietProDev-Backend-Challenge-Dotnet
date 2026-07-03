using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.PurchaseOrders;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireOrderRead")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var pos = await _purchaseOrderService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all purchase orders successfully.",
                data = pos,
                errors = (object?)null
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] PurchaseOrderQueryDto query, CancellationToken cancellationToken)
        {
            var pagedResult = await _purchaseOrderService.GetPagedAsync(query, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved purchase orders successfully.",
                data = pagedResult,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var po = await _purchaseOrderService.GetByIdAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved purchase order successfully.",
                data = po,
                errors = (object?)null
            });
        }

        [HttpPost]
        [Authorize(Policy = "RequireOrderCreate")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
        {
            var created = await _purchaseOrderService.CreateAsync(request, cancellationToken);
            return StatusCode(201, new
            {
                success = true,
                message = "Purchase order created successfully.",
                data = created,
                errors = (object?)null
            });
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Policy = "RequireOrderApprove")]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
        {
            await _purchaseOrderService.ConfirmAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Purchase order confirmed and inventory imported successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Policy = "RequireOrderApprove")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            await _purchaseOrderService.CancelAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Purchase order cancelled successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
