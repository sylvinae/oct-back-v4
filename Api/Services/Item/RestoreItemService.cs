using API.Db;
using API.Interfaces;
using API.Interfaces.Item;
using API.Models.Item;
using API.Utils;

namespace API.Services.Item;

public class RestoreItemService(
    Context _db,
    ILogger<RestoreItemService> _log,
    IItemHistoryService _ih
) : IRestoreItemService
{
    public async Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds)
    {
        var (failed, restored) = (new List<Guid>(), new List<Guid>());

        if (itemIds == null || itemIds.Count == 0)
        {
            _log.LogInformation("No items to delete.");
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
            var item = await _db.Items.FindAsync(id);
            if (item == null)
            {
                _log.LogWarning("Item with ID {ItemId} not found.", id);
                continue;
            }

            item.IsDeleted = true;
            restored.Add(id);

            await _ih.AddItemHistory(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = item.Hash }
                ),
                ActionType.Updated
            );
        }

        await _db.SaveChangesAsync();

        return restored;
    }
}
