using API.Filters;
using API.Interfaces;
using API.Models.Item;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var itemGroup = app.MapGroup("/api/items");
        //Get Routes
        itemGroup.MapGet("/{id}", GetItem).AllowAnonymous();
        itemGroup.MapGet("/", GetItems).AllowAnonymous();
        itemGroup.MapPost("/search", SearchItems).AllowAnonymous();
        itemGroup.MapPost("/", CreateItems).RequireAuthorization("admin");
        itemGroup.MapPut("/", UpdateItems).RequireAuthorization("admin");
        itemGroup.MapDelete("/", DeleteItems).RequireAuthorization("admin");
        itemGroup.MapPut("/restore/", RestoreItems).RequireAuthorization("admin");
    }

    private static async Task<IResult> GetItems(
        [FromServices] IItemService itemService,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] bool includeHistory = false,
        [FromQuery] bool isDeleted = false,
        [FromQuery] bool isExpired = false
    )
    {
        try
        {
            var (response, totalCount) = await itemService.GetItems(
                page,
                limit,
                includeHistory,
                isDeleted,
                isExpired
            );
            return totalCount == 0
                ? Results.NotFound()
                : Results.Ok(
                    new
                    {
                        items = response,
                        totalCount,
                        page,
                        limit,
                        includeHistory,
                    }
                );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.ToString() });
        }
    }

    private static async Task<IResult> GetItem(
        Guid id,
        [FromServices] IItemService itemService,
        [FromQuery] bool includeHistory = false
    )
    {
        try
        {
            var item = await itemService.GetItem(id, includeHistory);
            return item is not null ? Results.Ok(new { item, includeHistory }) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.ToString() });
        }
    }

    private static async Task<IResult> CreateItems(
        [FromBody] List<CreateItemModel> items,
        [FromServices] IItemService itemService
    )
    {
        var (failed, created) = await itemService.CreateItems(items);

        if (created.Count == 0)
            return Results.BadRequest(new { failed });
        if (failed.Count != 0)
            return Results.Ok(new { failed, created });

        return Results.Created("/items", new { created });
    }

    private static async Task<IResult> UpdateItems(
        [FromBody] List<UpdateItemModel> updateItemModel,
        [FromServices] IItemService itemService
    )
    {
        var (failed, updated) = await itemService.UpdateItems(updateItemModel);
        if (updated.Count == 0)
            return Results.BadRequest(new { failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed, updated });

        return Results.Ok(new { updated });
    }

    private static async Task<IResult> DeleteItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, deleted) = await itemService.DeleteItems(ids);
        if (deleted.Count == 0)
            return Results.BadRequest(new { failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed });

        return Results.Ok(new { deleted });
    }

    private static async Task<IResult> RestoreItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, restored) = await itemService.RestoreItems(ids);
        if (restored.Count == 0)
            return Results.BadRequest(new { failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed, restored });

        return Results.Ok(new { restored });
    }

    private static async Task<IResult> SearchItems(
        [FromServices] IItemService itemService,
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] bool isdeleted = false,
        [FromQuery] bool isExpired = false,
        [FromQuery] bool isReagent = false,
        [FromQuery] bool isLow = false,
        [FromQuery] bool includeHistory = false,
        [FromQuery] bool? hasExpiry = null
    )
    {
        var (response, totalCount) = await itemService.SearchItems(
            query,
            page,
            limit,
            isdeleted,
            isExpired,
            isReagent,
            isLow,
            includeHistory,
            hasExpiry
        );
        return totalCount == 0 ? Results.NotFound() : Results.Ok(new { response, totalCount });
    }
}
