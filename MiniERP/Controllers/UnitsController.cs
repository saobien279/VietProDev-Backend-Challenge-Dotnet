using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Units;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;
        private readonly IValidator<CreateUnitRequest> _createValidator;
        private readonly IValidator<UpdateUnitRequest> _updateValidator;

        public UnitsController(
            IUnitService unitService,
            IValidator<CreateUnitRequest> createValidator,
            IValidator<UpdateUnitRequest> updateValidator)
        {
            _unitService = unitService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var units = await _unitService.GetAllAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Retrieved all units successfully.",
                data = units,
                errors = (object?)null
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var unit = await _unitService.GetByIdAsync(id, cancellationToken);
            if (unit == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Unit with ID {id} not found.",
                    data = (object?)null,
                    errors = new[] { $"Unit with ID {id} does not exist or has been deleted." }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Retrieved unit successfully.",
                data = unit,
                errors = (object?)null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
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

            var created = await _unitService.CreateAsync(request, cancellationToken);
            return StatusCode(201, new
            {
                success = true,
                message = "Unit created successfully.",
                data = created,
                errors = (object?)null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext<UpdateUnitRequest>(request);
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
                var updated = await _unitService.UpdateAsync(id, request, cancellationToken);
                if (!updated)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Unit with ID {id} not found.",
                        data = (object?)null,
                        errors = new[] { $"Unit with ID {id} does not exist or has been deleted." }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Unit updated successfully.",
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
                var deleted = await _unitService.DeleteAsync(id, cancellationToken);
                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Unit with ID {id} not found.",
                        data = (object?)null,
                        errors = new[] { $"Unit with ID {id} does not exist or has been deleted." }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Unit deleted successfully (soft delete).",
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
