using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Products;
using MiniERP.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var products = await _productService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all products successfully.",
                data = products,
                errors = (object?)null
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] ProductQueryDto query, CancellationToken cancellationToken)
        {
            var pagedResult = await _productService.GetPagedAsync(query, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved products successfully.",
                data = pagedResult,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Product with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved product successfully.",
                data = product,
                errors = (object?)null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var created = await _productService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                success = true,
                message = "Product created successfully.",
                data = created,
                errors = (object?)null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var updated = await _productService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Product with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product updated successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _productService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Product with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Product with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product deleted successfully (soft delete).",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
