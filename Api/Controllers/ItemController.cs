using API.Entities.Item;
using API.Models.Item;
using API.Services.Item.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers;

[Route("api/items")]
[ApiController]
public class ItemController(
    ICreateItemService c,
    IGetItemService g,
    IUpdateItemService u,
    IDeleteItemService d,
    IRestoreItemService re,
    IRestockItemService rs) : ControllerBase
{
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<ItemEntity>> GetAllItems()
    {
        return Ok(g.GetAllItems());
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateItems([FromBody] List<CreateItemModel> items)
    {
        if (items.Count == 0)
            return BadRequest(new { status = "error" });

        var (ok, fails) = await c.CreateItems(items);

        if (ok.Count == 0 && fails.Count > 0)
            return BadRequest(new { status = "fail", data = new { fails } });

        if (ok.Count != 0 && fails.Count > 0)
            return StatusCode(207, new { status = "partial", data = new { ok, fails } });

        return Ok(new { status = "success", data = ok });
    }

    [Authorize(Roles = "admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateItems([FromBody] List<UpdateItemModel> items)
    {
        if (items.Count == 0)
            return BadRequest(new { status = "fail" });

        var (ok, fails) = await u.UpdateItems(items);

        if (ok.Count == 0 && fails.Count > 0)
            return BadRequest(new { status = "fail", errors = fails });

        if (ok.Count != 0 && fails.Count > 0)
            return StatusCode(207, new { status = "partial", data = new { ok, fails } });

        return Ok(new { status = "success", data = ok });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteItems([FromBody] List<Guid> ids)
    {
        if (ids.Count == 0)
            return BadRequest(new { status = "fail" });

        var (ok, fails) = await d.DeleteItems(ids);

        if (ok.Count == 0 && fails.Count > 0)
            return BadRequest(new { status = "fail", errors = fails });

        if (ok.Count != 0 && fails.Count > 0)
            return StatusCode(207, new { status = "partial", data = new { ok, fails } });

        return Ok(new { status = "success", data = ok });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("restore")]
    public async Task<IActionResult> RestoreItems([FromBody] List<Guid> ids)
    {
        if (ids.Count == 0)
            return BadRequest(new { status = "fail" });

        var (ok, fails) = await re.RestoreItems(ids);
        if (ok.Count == 0 && fails.Count > 0)
            return BadRequest(new { status = "fail", errors = fails });

        if (ok.Count != 0 && fails.Count > 0)
            return StatusCode(207, new { status = "partial", data = new { ok, fails } });

        return Ok(new { status = "success", data = ok });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("restock")]
    public async Task<IActionResult> RestockItems([FromBody] List<CreateRestockItemModel> items)
    {
        if (items.Count == 0)
            return BadRequest(new { status = "fail" });

        var (ok, fails) = await rs.RestockItemsAsync(items);

        if (ok.Count == 0 && fails.Count > 0)
            return BadRequest(new { status = "fail", errors = fails });

        if (ok.Count != 0 && fails.Count > 0)
            return StatusCode(207, new { status = "partial", data = new { ok, fails } });

        return Ok(new { status = "success", data = ok });
    }
}