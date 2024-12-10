using API.Db;
using API.Interfaces;
using API.Interfaces.Item;
using API.Models.Item;
using API.Utils;

namespace API.Services.Item;

public class DeleteItemService(
    Context db,
    ILogger<DeleteItemService> log,
    IItemHistoryService ih
) : IDeleteItemService
{
    public async Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds)
    {
        var (failed, deleted) = (new List<Guid>(), new List<Guid>());

        if (itemIds.Count == 0)
        {
            log.LogInformation("No items to delete.");
            return (failed, deleted);
        }
        deleted = await ProcessDeletion(itemIds);

        return (failed, deleted);
    }

    private async Task<List<Guid>> ProcessDeletion(List<Guid> itemIds)
    {
        var deleted = new List<Guid>();

        foreach (var id in itemIds)
        {
            var item = await db.Items.FindAsync(id);
            if (item == null)
            {
                log.LogWarning("Item with ID {ItemId} not found.", id);
                continue;
            }

            item.IsDeleted = true;
            deleted.Add(id);

            await ih.AddItemHistory(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = item.Hash }
                ),
                ActionType.Updated
            );
        }

        await db.SaveChangesAsync();

        return deleted;
    }
}
