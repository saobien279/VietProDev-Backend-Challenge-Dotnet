using FluentValidation;
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
        private readonly IValidator<CreateCategoryRequest> _createValidator;
        private readonly IValidator<UpdateCategoryRequest> _updateValidator;

        public CategoriesController(
            ICategoryService categoryService,
            IValidator<CreateCategoryRequest> createValidator,
            IValidator<UpdateCategoryRequest> updateValidator)
        {
            _categoryService = categoryService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
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
            // Pass route id into validation context for unique-name & circular-ref checks
            var validationContext = new ValidationContext<UpdateCategoryRequest>(request);
            validationContext.RootContextData["Id"] = id;

            var validationResult = await _updateValidator.ValidateAsync(validationContext, cancellationToken);
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
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
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
