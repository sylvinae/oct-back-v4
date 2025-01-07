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
            return BadRequest();

        var fails = await c.CreateItems(items);

        if (fails is { Count: > 0 })
            return BadRequest(fails);

        if (fails is { Count: > 0 })
            return StatusCode(207, fails);

        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateItems([FromBody] UpdateItemModel item)
    {
        var fail = await u.UpdateItem(item);

        if (fail is null)
            return BadRequest(fail);

        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteItems([FromBody] List<Guid> ids)
    {
        if (ids.Count == 0)
            return BadRequest();

        var fails = await d.DeleteItems(ids);

        if (fails is { Count: > 0 })
            return BadRequest(fails);

        if (fails is { Count: > 0 })
            return StatusCode(207, fails);

        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("restore")]
    public async Task<IActionResult> RestoreItems([FromBody] List<Guid> ids)
    {
        if (ids.Count == 0)
            return BadRequest();

        var fails = await re.RestoreItems(ids);
        if (fails is { Count: > 0 })
            return BadRequest(fails);

        if (fails is { Count: > 0 })
            return StatusCode(207, fails);

        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("restock")]
    public async Task<IActionResult> RestockItems([FromBody] List<CreateRestockItemModel> items)
    {
        if (items.Count == 0)
            return BadRequest();

        var fails = await rs.RestockItemsAsync(items);
        if (fails is { Count: > 0 })
            return BadRequest();

        if (fails is { Count: > 0 })
            return StatusCode(207, fails);

        return Ok();
    }
}