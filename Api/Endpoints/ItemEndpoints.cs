using System.Security.Claims;
using API.Interfaces;
using API.Models.Item;
using Data.Entities.Item;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var itemGroup = app.MapGroup("/api/items");

        itemGroup.MapGet("/some", GetItems).AllowAnonymous();
        itemGroup.MapGet("/", GetItem).AllowAnonymous();
        itemGroup.MapPost("/", CreateItems).RequireAuthorization("admin");
        itemGroup.MapPut("/", UpdateItems).RequireAuthorization("admin");
        itemGroup.MapDelete("/", DeleteItems).RequireAuthorization("admin");
        itemGroup.MapPut("/undelete/", UndeleteItems).RequireAuthorization("admin");
    }

    private static async Task<IResult> GetItems(
        [FromServices] IItemService itemService,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] bool includeHistory = false
    )
    {
        var (response, totalCount) = await itemService.GetItems(page, limit, includeHistory);
        return totalCount == 0 ? Results.NotFound() : Results.Ok(new { response, totalCount });
    }

    private static async Task<IResult> GetItem(
        [FromQuery] Guid id,
        [FromServices] IItemService itemService,
        [FromQuery] bool includeHistory = false
    )
    {
        var item = await itemService.GetItem(id, includeHistory);
        return item is not null ? Results.Ok(item) : Results.NotFound();
    }

    private static async Task<IResult> CreateItems(
        [FromBody] List<CreateItemModel> items,
        [FromServices] IItemService itemService
    )
    {
        var (failed, created) = await itemService.CreateItems(items);
        if (created.Count == 0)
            return Results.BadRequest(new { message = "Failed to create all items" });
        if (failed.Count != 0)
            return Results.Ok(new { message = "Failed to create some items.", failed });

        return Results.Ok(new { message = "All items created successfully." });
    }

    private static async Task<IResult> UpdateItems(
        [FromBody] List<UpdateItemModel> updateItemModel,
        [FromServices] IItemService itemService
    )
    {
        var (failed, updated) = await itemService.UpdateItems(updateItemModel);
        if (updated.Count == 0)
            return Results.BadRequest(new { message = "Failed to update all items" });
        if (failed.Count != 0)
            return Results.Ok(new { message = "Failed to update some items.", failed });

        return Results.Ok(new { message = "All items updated successfully." });
    }

    private static async Task<IResult> DeleteItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, deleted) = await itemService.DeleteItems(ids);
        if (deleted.Count == 0)
            return Results.BadRequest(new { message = "Failed to delete all items" });
        if (failed.Count != 0)
            return Results.Ok(new { message = "Failed to delete some items.", failed });

        return Results.Ok(new { message = "All items deleted successfully." });
    }

    private static async Task<IResult> UndeleteItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, undeleted) = await itemService.UndeleteItems(ids);
        if (undeleted.Count == 0)
            return Results.BadRequest(new { message = "Failed to undelete all items" });
        if (failed.Count != 0)
            return Results.Ok(new { message = "Failed to undelete some items.", failed });

        return Results.Ok(new { message = "All items undeleted successfully." });
    }
}
