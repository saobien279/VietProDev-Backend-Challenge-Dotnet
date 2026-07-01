using System;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using MiniERP.Application.Interfaces.Services;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        [HttpPost("daily-summary/run")]
        public IActionResult RunDailySummary([FromQuery] DateTime? targetDate = null)
        {
            // Enqueue Hangfire background job
            BackgroundJob.Enqueue<IDailySummaryJob>(job => job.ExecuteAsync(targetDate));
            
            return Ok(new
            {
                success = true,
                message = $"Daily summary job triggered successfully{(targetDate.HasValue ? $" for date {targetDate.Value:yyyy-MM-dd}" : " for yesterday")}.",
                data = (object?)null,
                errors = (object?)null
            });
        }
    }
}
