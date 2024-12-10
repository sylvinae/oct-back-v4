using API.Db;
using API.Interfaces;
using API.Interfaces.Item;
using API.Models.Item;
using API.Utils;

namespace API.Services.Item;

public class RestoreItemService(
    Context db,
    ILogger<RestoreItemService> log,
    IItemHistoryService ih
) : IRestoreItemService
{
    public async Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds)
    {
        var (failed, restored) = (new List<Guid>(), new List<Guid>());

        if (itemIds.Count == 0)
        {
            log.LogInformation("No items to delete.");
            return (failed, restored);
        }

        restored = await ProcessRestoration(itemIds);

        return (failed, restored);
    }

    private async Task<List<Guid>> ProcessRestoration(List<Guid> itemIds)
    {
        var restored = new List<Guid>();

        foreach (var id in itemIds)
        {
            var item = await db.Items.FindAsync(id);
            if (item == null)
            {
                log.LogWarning("Item with ID {ItemId} not found.", id);
                continue;
            }

            item.IsDeleted = true;
            restored.Add(id);

            await ih.AddItemHistory(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = item.Hash }
                ),
                ActionType.Updated
            );
        }

        await db.SaveChangesAsync();

        return restored;
    }
}