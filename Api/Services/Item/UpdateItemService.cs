using API.Db;
using API.Entities.Item;
using API.Models;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Item;

public class UpdateItemService(
    Context db,
    ILogger<UpdateItemService> log,
    IItemHistoryService ih,
    IValidator<UpdateItemModel> updateValidator
) : IUpdateItemService
{
    public async Task<BulkFailure<UpdateItemModel>?> UpdateItem(UpdateItemModel item)
    {
        log.LogInformation("Updating product {ItemId}", item.Id);

        var validationResult = await updateValidator.ValidateAsync(item);
        if (!validationResult.IsValid)
            return new BulkFailure<UpdateItemModel>
            {
                Input = item,
                Errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            };


        var existingItem = await db.Products.OfType<ItemEntity>().FirstOrDefaultAsync(i => i.Id == item.Id);

        if (existingItem == null)
            return new BulkFailure<UpdateItemModel>
            {
                Input = item,
                Errors = new Dictionary<string, string> { { item.Id.ToString(), "Item not found." } }
            };


        var hash = Cryptics.ComputeHash(item);
        existingItem.Hash = hash;
        db.Entry(existingItem).CurrentValues.SetValues(item);


        var history = PropCopier.Copy(existingItem,
            new CreateItemHistoryModel { ItemId = existingItem.Id, Action = Actions.Updated.ToString() });

        await ih.AddItemHistory(history);

        try
        {
            await db.SaveChangesAsync();
            log.LogInformation("Item with ID: {ItemId} successfully updated.", item.Id);
            return null;
        }
        catch (Exception ex)
        {
            log.LogError("Error updating item with ID: {ItemId}. Exception: {Exception}", item.Id, ex);
            return new BulkFailure<UpdateItemModel>
            {
                Input = item,
                Errors = new Dictionary<string, string> { { "SaveChanges", ex.Message } }
            };
        }
    }
}