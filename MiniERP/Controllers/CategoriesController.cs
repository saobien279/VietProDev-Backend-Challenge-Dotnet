using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Categories;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all categories successfully.",
                data = categories,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Category with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Category with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved category successfully.",
                data = category,
                errors = (object?)null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var created = await _categoryService.CreateAsync(request, cancellationToken);
            return StatusCode(201, new
            {
                success = true,
                message = "Category created successfully.",
                data = created,
                errors = (object?)null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var updated = await _categoryService.UpdateAsync(id, request, cancellationToken);
            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Category with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Category with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Category updated successfully.",
                data = (object?)null,
                errors = (object?)null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _categoryService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Category with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Category with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Category deleted successfully (soft delete).",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
