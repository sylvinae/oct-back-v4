using API.Entities.Bundles;
using API.Models.Bundles;
using API.Services.Bundle.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers;

[Route("api/bundles")]
[ApiController]
public class BundleController(ICreateBundleService c, IGetBundleService g) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBundle([FromBody] CreateBundleModel bundle)
    {
        var ok = await c.CreateBundle(bundle);
        if (ok) return Ok();
        return BadRequest();
    }

    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<BundleEntity>> GetAllBundles()
    {
        return Ok(g.GetAllBundles());
    }
}