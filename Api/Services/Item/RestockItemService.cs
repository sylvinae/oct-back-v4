using API.Db;
using API.Entities.Item;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;

namespace API.Services.Item;

public class RestockItemService(
    Context db,
    ILogger<CreateItemService> log,
    IValidator<CreateRestockItemModel> createValidator,
    IItemHistoryService ih) : IRestockItemService
{
    public async
        Task<bool>
        RestockItemsAsync(
            List<CreateRestockItemModel> restockItems)
    {
        log.LogInformation("Restocking items...");

        var toCreate = new List<ItemEntity>();
        var toAddCreate = new List<AddItemHistoryModel>();
        var toAddRestock = new List<AddItemHistoryModel>();
        var existingHashes = new HashSet<string>(restockItems.Select(Cryptics.ComputeHash).ToList());
        var existingItems = db.Items
            .Where(i => existingHashes.Contains(i.Hash))
            .ToList();

        foreach (var item in restockItems)
        {
            var result = await createValidator.ValidateAsync(item);
            if (!result.IsValid)
                return false;

            var hash = Cryptics.ComputeHash(item);
            var existingItem = existingItems.FirstOrDefault(i => i.Hash == hash);
            if (existingItem == null)
            {
                log.LogInformation("New item detected. Creating {x}, {y}...", item.Brand, item.Generic);
                var newItem = PropCopier.Copy(item,
                    new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold });
                toCreate.Add(newItem);
                toAddCreate.Add(PropCopier.Copy(item, new AddItemHistoryModel
                {
                    ItemId = newItem.Id, Hash = hash, Action = ActionType.Created.ToString()
                }));
            }
            else
            {
                log.LogInformation("Restocking {x}...", existingItem.Id);
                existingItem.Stock += item.Stock;
                toAddRestock.Add(PropCopier.Copy(item,
                    new AddItemHistoryModel { ItemId = existingItem.Id, Action = ActionType.Restored.ToString() }));
            }
        }

        var addItemsTask = db.Items.AddRangeAsync(toCreate);
        var addCreatedHistoryTask = ih.AddItemHistoryRange(toAddCreate, ActionType.Created);
        var addRestockHistoryTask = ih.AddItemHistoryRange(toAddRestock, ActionType.Restocked);

        await Task.WhenAll(addItemsTask, addCreatedHistoryTask, addRestockHistoryTask);

        await db.SaveChangesAsync();
        log.LogInformation("Restocked {x} items. Added {y} items.", toAddRestock.Count, toAddCreate.Count);
        return true;
    }
}