using API.Models.Bundles;
using API.Services.Bundle.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/bundles")]
[ApiController]
public class BundleController(ICreateBundleService c) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBundle([FromBody] CreateBundleModel bundle)
    {
        var ok = await c.CreateBundle(bundle);
        if (ok) return Ok();
        return BadRequest();
    }
}