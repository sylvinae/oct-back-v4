using API.Db;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Item;

public class DeleteItemService(
    Context db,
    ILogger<DeleteItemService> log,
    IItemHistoryService ih
) : IDeleteItemService
{
    public async Task<bool> DeleteItems(List<Guid> itemIds)
    {
        if (itemIds.Count == 0)
        {
            log.LogInformation("No items to delete.");
            return false;
        }

        var items = await db.Items.Where(item => itemIds.Contains(item.Id)).ToListAsync();
        var toAdd = new List<AddItemHistoryModel>();
        if (items.Count == 0)
            return false;
        foreach (var item in items)
        {
            item.IsDeleted = true;
            var hash = Cryptics.ComputeHash(item);
            toAdd.Add(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = hash }
                )
            );
        }

        var result = await ih.AddItemHistoryRange(toAdd, ActionType.Deleted);
        if (!result)
            return false;

        await db.SaveChangesAsync();
        log.LogInformation("Deletion completed.");
        return true;
    }
}