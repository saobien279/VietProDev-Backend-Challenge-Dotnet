using Microsoft.AspNetCore.Mvc;

namespace MiniERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                success = true,
                message = "Healthy",
                data = new { status = "API is running smoothly" },
                errors = (object?)null
            });
        }
    }
}

