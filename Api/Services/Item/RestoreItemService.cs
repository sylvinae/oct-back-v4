using API.Db;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;

namespace API.Services.Item;

public class RestoreItemService(
    Context db,
    ILogger<RestoreItemService> log,
    IItemHistoryService ih
) : IRestoreItemService
{
    public async Task<bool> RestoreItems(List<Guid> itemIds)
    {
        if (itemIds.Count == 0)
        {
            log.LogInformation("No items to Restore.");
            return false;
        }

        var items = db.Items.Where(item => itemIds.Contains(item.Id)).ToList();
        var toAdd = new List<AddItemHistoryModel>();
        if (items.Count == 0)
            return false;
        foreach (var item in items)
        {
            item.IsDeleted = false;
            var hash = Cryptics.ComputeHash(item);
            toAdd.Add(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = hash }
                )
            );
        }

        var result = await ih.AddItemHistoryRange(toAdd, ActionType.Restored);
        if (!result)
            return false;

        await db.SaveChangesAsync();
        log.LogInformation("Restoration completed.");
        return true;
    }
}