using API.Db;
using API.Entities.Item;
using API.Models;
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
    public async Task<List<BulkFailure<CreateItemModel>>?> CreateItems(
        List<CreateItemModel> items)
    {
        log.LogInformation("Creating items...");

        var toCreate = new List<ItemEntity>(); // Using ItemEntity
        var toAddHistory = new List<AddItemHistoryModel>();
        var fails = new List<BulkFailure<CreateItemModel>>();
        var itemHashes = items.Select(Cryptics.ComputeHash).ToHashSet();

        // Check for existing hashes in the Products table, specifically looking for ItemEntity instances
        var existingHashes = db.Products
            .OfType<ItemEntity>() // Filter for ItemEntity type
            .Where(i => itemHashes.Contains(i.Hash))
            .Select(i => i.Hash)
            .ToHashSet();

        foreach (var item in items)
        {
            var hash = Cryptics.ComputeHash(item);
            if (existingHashes.Contains(hash))
            {
                log.LogInformation("Skipping item with hash {x}...", hash);
                fails.Add(new BulkFailure<CreateItemModel>
                {
                    Input = item,
                    Errors = new Dictionary<string, string>
                        { { "item", "Item already exists." } }
                });
                continue;
            }

            // Validate the incoming item model
            var isValid = await createValidator.ValidateAsync(item);
            if (!isValid.IsValid)
            {
                fails.Add(new BulkFailure<CreateItemModel>
                {
                    Input = item,
                    Errors = isValid.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
                });
                continue;
            }

            // Create ItemEntity from CreateItemModel
            var newItem = PropCopier.Copy(item, new ItemEntity
            {
                Hash = hash,
                IsLow = item.Stock <= item.LowThreshold // Mark if stock is low
            });


            toCreate.Add(newItem);

            toAddHistory.Add(new AddItemHistoryModel
            {
                ItemId = newItem.Id,
                Action = ActionType.Created.ToString()
            });
        }

        if (toCreate.Count == 0) return fails;

        // Add the newly created items to the Products table
        await db.AddRangeAsync(toCreate);
        await ih.AddItemHistoryRange(toAddHistory);
        await db.SaveChangesAsync();

        log.LogInformation("Created {x} items", toCreate.Count);
        return fails;
    }
}