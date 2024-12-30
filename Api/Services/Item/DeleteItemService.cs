using API.Db;
using API.Entities.Item;
using API.Models;
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
    public async Task<List<BulkFailure<Guid>>?> DeleteItems(List<Guid> itemIds)
    {
        log.LogInformation("Processing deletion of {Count} items.", itemIds.Count);

        var items = await db.Products.OfType<ItemEntity>().Where(item => itemIds.Contains(item.Id)).ToListAsync();
        var existingItemIds = items.Select(item => item.Id).ToHashSet();

        var fails = new List<BulkFailure<Guid>>();
        var toAddHistory = new List<AddItemHistoryModel>();

        foreach (var id in itemIds)
        {
            if (!existingItemIds.Contains(id))
            {
                fails.Add(new BulkFailure<Guid>
                {
                    Input = id,
                    Errors = new Dictionary<string, string> { { "id", "Item not found." } }
                });
                continue;
            }

            var item = items.First(i => i.Id == id);

            if (item.IsDeleted)
            {
                fails.Add(new BulkFailure<Guid>
                {
                    Input = id,
                    Errors = new Dictionary<string, string> { { "id", "Item is already deleted." } }
                });
                continue;
            }

            item.IsDeleted = true;

            var hash = Cryptics.ComputeHash(item);
            toAddHistory.Add(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel
                        { ItemId = item.Id, Hash = hash, Action = ActionType.Deleted.ToString() }
                )
            );
        }

        if (toAddHistory.Count == 0) return fails;
        var result = await ih.AddItemHistoryRange(toAddHistory);
        if (!result)
        {
            log.LogError("Failed to add deletion history.");
            throw new Exception("History addition failed.");
        }

        await db.SaveChangesAsync();
        log.LogInformation("Deletion completed.");

        return fails;
    }
}