using API.Db;
using API.Entities.Item;
using API.Models;
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
        Task<List<BulkFailure<CreateRestockItemModel>>?>
        RestockItemsAsync(
            List<CreateRestockItemModel> restockItems)
    {
        log.LogInformation("Restocking items...");

        var toCreate = new List<ItemEntity>();
        var toAddHistory = new List<CreateItemHistoryModel>();

        var fails = new List<BulkFailure<CreateRestockItemModel>>();

        var existingHashes = new HashSet<string>(restockItems.Select(Cryptics.ComputeHash).ToList());
        var existingItems = db.Products.OfType<ItemEntity>()
            .Where(i => existingHashes.Contains(i.Hash))
            .ToList();

        foreach (var item in restockItems)
        {
            var isValid = await createValidator.ValidateAsync(item);
            if (!isValid.IsValid)
            {
                fails.Add(new BulkFailure<CreateRestockItemModel>
                {
                    Input = item,
                    Errors = isValid.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
                });
                continue;
            }

            var hash = Cryptics.ComputeHash(item);
            var existingItem = existingItems.FirstOrDefault(i => i.Hash == hash);
            if (existingItem == null)
            {
                log.LogInformation("New item detected. Will create {x}, {y}...", item.Brand, item.Generic);
                var newItem = PropCopier.Copy(item,
                    new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold });
                toCreate.Add(newItem);
                toAddHistory.Add(PropCopier.Copy(item, new CreateItemHistoryModel
                {
                    ItemId = newItem.Id, Hash = hash, Action = Actions.Created.ToString()
                }));
            }
            else
            {
                log.LogInformation("Restocking {x}...", existingItem.Id);
                existingItem.Stock += item.Stock;
                item.Stock = existingItem.Stock;
                toAddHistory.Add(PropCopier.Copy(item,
                    new CreateItemHistoryModel
                        { ItemId = existingItem.Id, Action = Actions.Restocked.ToString() }));
            }
        }

        if (toAddHistory.Count == 0) return fails;

        var addItemsTask = db.Products.AddRangeAsync(toCreate);
        var addCreatedHistoryTask = ih.AddItemHistoryRange(toAddHistory);

        await Task.WhenAll(addItemsTask, addCreatedHistoryTask);

        await db.SaveChangesAsync();
        log.LogInformation("Restocked {x} items. Added {y} items.", toAddHistory.Count, toAddHistory.Count);

        return fails;
    }
}