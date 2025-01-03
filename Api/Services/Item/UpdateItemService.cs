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
    public async Task<List<BulkFailure<UpdateItemModel>>?
    > UpdateItems(
        List<UpdateItemModel> items)
    {
        log.LogInformation("Updating items...");
        var toAddHistory = new List<AddItemHistoryModel>();
        var fails = new List<BulkFailure<UpdateItemModel>>();

        foreach (var item in items)
        {
            var isValid = await updateValidator.ValidateAsync(item);
            if (!isValid.IsValid)
            {
                fails.Add(new BulkFailure<UpdateItemModel>
                {
                    Input = item,
                    Errors = isValid.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
                });
                continue;
            }

            var existingItem = await db.Products.OfType<ItemEntity>().FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existingItem == null)
            {
                fails.Add(new BulkFailure<UpdateItemModel>
                {
                    Input = item,
                    Errors = isValid.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
                });
                continue;
            }

            var hash = Cryptics.ComputeHash(item);
            existingItem.Hash = hash;
            db.Entry(existingItem).CurrentValues.SetValues(item);
            toAddHistory.Add(PropCopier.Copy(existingItem,
                new AddItemHistoryModel { ItemId = existingItem.Id, Action = Actions.Updated.ToString() })
            );
        }

        if (toAddHistory.Count == 0) return fails;
        await ih.AddItemHistoryRange(toAddHistory);
        await db.SaveChangesAsync();
        log.LogInformation("Updated {x} items.", items.Count);
        return fails;
    }
}