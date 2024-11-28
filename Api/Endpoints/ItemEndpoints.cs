using System.Collections.Generic;
using System.Security.Claims;
using API.Filters;
using API.Interfaces;
using API.Models.Item;
using API.Services;
using API.Validators;
using Data.Entities.Item;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;

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

        //Create Routes
        itemGroup.MapPost("/", CreateItem).AddEndpointFilter<ValidationFilter<CreateItemModel>>();
        itemGroup.MapPost("/batch", CreateItems);

        // Update Routes
        itemGroup.MapPut("/", UpdateItem).AddEndpointFilter<ValidationFilter<UpdateItemModel>>();
        itemGroup.MapPut("/batch", UpdateItems);

        // Delete Routes
        itemGroup.MapDelete("/{id}", DeleteItem);
        itemGroup.MapDelete("/batch", DeleteItems);

        // Restore Routes
        itemGroup.MapPut("/restore/{id}", RestoreItem);
        itemGroup.MapPut("/restore/batch", RestoreItems);
    }

    private static async Task<IResult> GetItems(
        [FromServices] IItemService itemService,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] bool includeHistory = false
    )
    {
        try
        {
            var (response, totalCount) = await itemService.GetItems(page, limit, includeHistory);
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
            return item is not null
                ? Results.Ok(new { item = item, includeHistory })
                : Results.NotFound();
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
            return Results.BadRequest(new { failed = failed });
        if (failed.Count != 0)
            return Results.Ok(new { failed = failed, created = created });

        return Results.Created("/items", new { created = created });
    }

    private static async Task<IResult> CreateItem(
        [FromBody] CreateItemModel item,
        [FromServices] IItemService itemService
    )
    {
        var (failed, created) = await itemService.CreateItem(item);
        return created != null
            ? Results.Created(created.Id.ToString(), new { created = created })
            : Results.BadRequest(new { failed = failed });
    }

    private static async Task<IResult> UpdateItems(
        [FromBody] List<UpdateItemModel> updateItemModel,
        [FromServices] IItemService itemService
    )
    {
        var (failed, updated) = await itemService.UpdateItems(updateItemModel);
        if (updated.Count == 0)
            return Results.BadRequest(new { failed = failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed = failed, updated = updated });

        return Results.Ok(new { updated = updated });
    }

    private static async Task<IResult> UpdateItem(
        [FromBody] UpdateItemModel updateItemModel,
        [FromServices] IItemService itemService
    )
    {
        var (failed, updated) = await itemService.UpdateItem(updateItemModel);

        return updated == null
            ? Results.BadRequest(new { failed = failed })
            : Results.Ok(new { updated = updated });
    }

    private static async Task<IResult> DeleteItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, deleted) = await itemService.DeleteItems(ids);
        if (deleted.Count == 0)
            return Results.BadRequest(new { failed = failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed = failed });

        return Results.Ok(new { deleted = deleted });
    }

    private static async Task<IResult> DeleteItem(Guid id, [FromServices] IItemService itemService)
    {
        var deleted = await itemService.DeleteItem(id);

        return deleted == null
            ? Results.BadRequest(new { failed = id })
            : Results.Ok(new { deleted = deleted });
    }

    private static async Task<IResult> RestoreItems(
        [FromBody] List<Guid> ids,
        [FromServices] IItemService itemService
    )
    {
        var (failed, restored) = await itemService.RestoreItems(ids);
        if (restored.Count == 0)
            return Results.BadRequest(new { failed = failed });
        if (failed.Count != 0)
            return Results.BadRequest(new { failed = failed, restored = restored });

        return Results.Ok(new { restored = restored });
    }

    private static async Task<IResult> RestoreItem(
        [FromBody] Guid id,
        [FromServices] IItemService itemService
    )
    {
        var restored = await itemService.RestoreItem(id);
        return restored == null
            ? Results.BadRequest(new { failed = id })
            : Results.Ok(new { restored = restored });
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
