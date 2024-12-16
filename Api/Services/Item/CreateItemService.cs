using API.Db;
using API.Entities.Item;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;

namespace API.Services.Item;

public class CreateItemService(
    Context db,
    ILogger<CreateItemService> log,
    IValidator<CreateItemModel> createValidator,
    IItemHistoryService ih
) : ICreateItemService
{
    public async Task<bool> CreateItems(List<CreateItemModel> items)
    {
        log.LogInformation("Creating items...");

        var toCreate = new List<ItemEntity>();
        var toAdd = new List<AddItemHistoryModel>();

        var itemHashes = items.Select(Cryptics.ComputeHash).ToHashSet();
        var existingHashes = db.Items
            .Where(i => itemHashes.Contains(i.Hash))
            .Select(i => i.Hash)
            .ToHashSet();


        foreach (var item in items)
        {
            var hash = Cryptics.ComputeHash(item);

            if (existingHashes.Contains(hash))
            {
                log.LogInformation("Skipping item with hash {x}...", hash);
                continue;
            }

            var isValid = await createValidator.ValidateAsync(item);
            if (!isValid.IsValid)
                return false;

            var newItem = PropCopier.Copy(item,
                new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold });
            toCreate.Add(newItem);
            toAdd.Add(PropCopier.Copy(newItem, new AddItemHistoryModel { ItemId = newItem.Id, Hash = hash }));
        }

        if (toCreate.Count == 0) return false;
        await db.AddRangeAsync(toCreate);
        await ih.AddItemHistoryRange(toAdd, ActionType.Created);

        await db.SaveChangesAsync();
        log.LogInformation("Created {x} items", toCreate.Count);
        return true;
    }
}