using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.DTOs.Units;
using MiniERP.Application.Interfaces.Services;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireReadAccess")]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
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
        [Authorize(Policy = "RequireWriteAccess")]
        public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
        {
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
        [Authorize(Policy = "RequireWriteAccess")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
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

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireWriteAccess")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
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
    }
}
