using API.Db;
using API.Models;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Item;

public class RestoreItemService(
    Context db,
    ILogger<RestoreItemService> log,
    IItemHistoryService ih
) : IRestoreItemService
{
    public async Task<(List<Guid> ok, List<BulkFailure<Guid>> fails)> RestoreItems(List<Guid> itemIds)
    {
        log.LogInformation("Processing deletion of {Count} items.", itemIds.Count);

        var items = await db.Items.Where(item => itemIds.Contains(item.Id)).ToListAsync();
        var existingItemIds = items.Select(item => item.Id).ToHashSet();

        var ok = new List<Guid>();
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

            if (!item.IsDeleted)
            {
                fails.Add(new BulkFailure<Guid>
                {
                    Input = id,
                    Errors = new Dictionary<string, string> { { "id", "Item is already restored." } }
                });
                continue;
            }

            item.IsDeleted = false;
            ok.Add(item.Id);

            var hash = Cryptics.ComputeHash(item);
            toAddHistory.Add(
                PropCopier.Copy(
                    item,
                    new AddItemHistoryModel { ItemId = item.Id, Hash = hash }
                )
            );
        }


        if (ok.Count <= 0) return (ok, fails);
        var result = await ih.AddItemHistoryRange(toAddHistory);
        if (!result)
        {
            log.LogError("Failed to add restoration history.");
            throw new Exception("History addition failed.");
        }

        await db.SaveChangesAsync();
        log.LogInformation("Restoration completed. {Count} items restored.", ok.Count);

        return (ok, fails);
    }
}