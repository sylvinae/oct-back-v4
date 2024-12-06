using API.Entities.Item;
using API.Interfaces;
using API.Models.Item;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers;

[Route("api/items")]
[ApiController]
public class ItemController(IItemService itemService) : ControllerBase
{
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<ItemEntity>> GetAllItems()
    {
        return Ok(itemService.GetAllItems());
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateItems([FromBody] List<CreateItemModel> items)
    {
        var (failed, created) = await itemService.CreateItems(items);

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

    [HttpPut]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateItems([FromBody] List<UpdateItemModel> updateItemModel)
    {
        var (failed, updated) = await itemService.UpdateItems(updateItemModel);
        if (updated.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed, updated });

        return Ok(new { updated });
    }

    [HttpDelete]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteItems([FromBody] List<Guid> ids)
    {
        var (failed, deleted) = await itemService.DeleteItems(ids);
        if (deleted.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed });

        return Ok(new { deleted });
    }

    [HttpPut("restore")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RestoreItems([FromBody] List<Guid> ids)
    {
        var (failed, restored) = await itemService.RestoreItems(ids);
        if (restored.Count == 0)
            return BadRequest(new { failed });
        if (failed.Count != 0)
            return BadRequest(new { failed, restored });

        return Ok(new { restored });
    }
}
