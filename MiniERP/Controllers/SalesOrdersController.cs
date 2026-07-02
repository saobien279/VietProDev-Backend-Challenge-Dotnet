using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Payments;
using MiniERP.Application.DTOs.SalesOrders;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF,ACCOUNTANT")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IPaymentService _paymentService;

        public SalesOrdersController(
            ISalesOrderService salesOrderService,
            IPaymentService paymentService)
        {
            _salesOrderService = salesOrderService;
            _paymentService = paymentService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var sos = await _salesOrderService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all sales orders successfully.",
                data = sos,
                errors = (object?)null
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] SalesOrderQueryDto query, CancellationToken cancellationToken)
        {
            var pagedResult = await _salesOrderService.GetPagedAsync(query, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved sales orders successfully.",
                data = pagedResult,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var so = await _salesOrderService.GetByIdAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved sales order successfully.",
                data = so,
                errors = (object?)null
            });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken)
        {
            var created = await _salesOrderService.CreateAsync(request, cancellationToken);
            return StatusCode(201, new
            {
                success = true,
                message = "Sales order created successfully.",
                data = created,
                errors = (object?)null
            });
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "ADMIN,MANAGER")]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
        {
            await _salesOrderService.ConfirmAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Sales order confirmed and stock exported successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "ADMIN,MANAGER")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            await _salesOrderService.CancelAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Sales order cancelled successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpGet("{id}/payments")]
        [Authorize(Roles = "ADMIN,ACCOUNTANT,STAFF")]
        public async Task<IActionResult> GetPayments(Guid id, CancellationToken cancellationToken)
        {
            var payments = await _paymentService.GetPaymentsBySalesOrderIdAsync(id, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved payments successfully.",
                data = payments,
                errors = (object?)null
            });
        }

        [HttpPost("{id}/payments")]
        [Authorize(Roles = "ADMIN,ACCOUNTANT")]
        public async Task<IActionResult> AddPayment(Guid id, [FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
        {
            var payment = await _paymentService.AddPaymentAsync(id, request, cancellationToken);
            return StatusCode(201, new
            {
                success = true,
                message = "Payment added successfully.",
                data = payment,
                errors = (object?)null
            });
        }
    }
}
