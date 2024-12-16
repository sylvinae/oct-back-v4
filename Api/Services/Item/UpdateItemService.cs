using API.Db;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;

namespace API.Services.Item;

public class UpdateItemService(
    Context db,
    ILogger<UpdateItemService> log,
    IItemHistoryService ih,
    IValidator<UpdateItemModel> updateValidator
) : IUpdateItemService
{
    public async Task<bool> UpdateItems(List<UpdateItemModel> items)
    {
        log.LogInformation("Updating items...");
        var toAdd = new List<AddItemHistoryModel>();
        foreach (var item in items)
        {
            var result = await updateValidator.ValidateAsync(item);
            if (!result.IsValid)
                return false;
            var existingItem = await db.Items.FindAsync(item.Id);
            if (existingItem == null)
                return false;

            var hash = Cryptics.ComputeHash(item);
            db.Entry(existingItem).CurrentValues.SetValues(item);
            existingItem.Hash = hash;
            toAdd.Add(PropCopier.Copy(existingItem, new AddItemHistoryModel { ItemId = existingItem.Id })
            );
        }

        await ih.AddItemHistoryRange(toAdd, ActionType.Updated);
        await db.SaveChangesAsync();
        log.LogInformation("Updated {x} items.", items.Count);
        return true;
    }
}