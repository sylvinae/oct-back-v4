using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet("error")]
        public IActionResult ReturnError()
        {
            return BadRequest(new { message = "dumb dev. fix url." });
        }
    }
}
