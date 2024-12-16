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
    IRestockItemService rs
) : ControllerBase
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
        var (failed, created) = await c.CreateItems(items);

        if (created.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return Ok(new { failed, created });

        return CreatedAtAction(
            nameof(CreateItems),
            new { id = created.First().Id },
            new { created }
        );
    }

    [Authorize(Roles = "admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateItems([FromBody] List<UpdateItemModel> items)
    {
        var (failed, updated) = await u.UpdateItems(items);
        if (updated.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed, updated });

        return Ok(new { updated });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteItems([FromBody] List<Guid> ids)
    {
        var (failed, deleted) = await d.DeleteItems(ids);
        if (deleted.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed });

        return Ok(new { deleted });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("restore")]
    public async Task<IActionResult> RestoreItems([FromBody] List<Guid> ids)
    {
        var (failed, restored) = await re.RestoreItems(ids);
        if (restored.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed, restored });

        return Ok(new { restored });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("restock")]
    public async Task<IActionResult> RestockItems([FromBody] List<CreateRestockItemModel> items)
    {
        var (failed, restocked, created) = await rs.RestockItemsAsync(items);
        if (restocked.Count == items.Count)
            return Ok(new { restocked });
        if (created.Count == items.Count)
            return Ok(new { created });
        if (failed.Count == items.Count) return BadRequest(new { failed });
        return Ok(new { failed, restocked, created });
    }
}