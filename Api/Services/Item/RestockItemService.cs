using API.Db;
using API.Entities.Item;
using API.Interfaces;
using API.Interfaces.Item;
using API.Models.Item;
using API.Utils;
using API.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Item;

public class RestockItemService(
    Context db,
    ILogger<CreateItemService> log,
    IValidator<CreateRestockItemModel> createValidator,
    IItemHistoryService ih) : IRestockItemService
{
    public async
        Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> restocked, List<ResponseItemModel> created)>
        RestockItemsAsync(
            List<CreateRestockItemModel> restockItems)
    {
        log.LogInformation("Restock items called");

        var failed = new List<FailedResponseItemModel>();
        var restocked = new List<ResponseItemModel>();
        var created = new List<ResponseItemModel>();
        if (restockItems.Count == 0)
        {
            log.LogInformation("No items to restock");
            return (failed, restocked, created);
        }

        foreach (var item in restockItems)
            try
            {
                var (isValid, validationError) = await ValidateItem(item);
                if (!isValid)
                {
                    AddToFailedList(item, validationError ?? "", failed);
                    continue;
                }

                var (restockedItem, createdItem) = await RestockItem(item);
                if (restockedItem != null) restocked.Add(restockedItem);
                if (createdItem != null) created.Add(createdItem);
            }
            catch (Exception ex)
            {
                log.LogError(
                    ex,
                    "An error occurred while restocking item {Brand} - {Generic}.",
                    item.Brand,
                    item.Generic
                );
                AddToFailedList(item, ex.Message, failed);
            }

        await db.SaveChangesAsync();
        log.LogInformation("Restocked items succesfully.");
        return (failed, restocked, created);
    }

    private async Task<(bool isValid, string?error)> ValidateItem(CreateRestockItemModel item)
    {
        log.LogInformation("Validating item {Brand} - {Generic}.", item.Brand, item.Generic);
        return await SuperValidator.Check(createValidator, item);
    }

    private void AddToFailedList(
        CreateRestockItemModel item,
        string error,
        List<FailedResponseItemModel> failed
    )
    {
        log.LogWarning(
            "Failed to process item {Brand} - {Generic}: {Error}.",
            item.Brand,
            item.Generic,
            error
        );
        failed.Add(PropCopier.Copy(item, new FailedResponseItemModel { Error = error }));
    }

    private async Task<(ResponseItemModel? restocked, ResponseItemModel? created)> RestockItem(
        CreateRestockItemModel item)
    {
        var hash = Cryptics.ComputeHash(item);
        var itemEntity = await db.Items.FirstOrDefaultAsync(i => i.Hash == hash);

        if (itemEntity == null)
        {
            itemEntity = PropCopier.Copy(item, new ItemEntity
            {
                Hash = hash, IsLow = item.Stock <= item.LowThreshold
            });
            var itemHistory = PropCopier.Copy(item, new AddItemHistoryModel { ItemId = itemEntity.Id, Hash = hash });

            await db.Items.AddAsync(itemEntity);
            await ih.AddItemHistory(itemHistory, ActionType.Created);

            var created = PropCopier.Copy(itemEntity, new ResponseItemModel());
            log.LogInformation("Created item with ID {ItemId}.", itemEntity.Id);
            return (null, created);
        }

        else
        {
            item.Stock += itemEntity.Stock;
            var itemHistory = PropCopier.Copy(item, new AddItemHistoryModel { ItemId = itemEntity.Id, Hash = hash });

            db.Entry(itemEntity).CurrentValues.SetValues(item);
            itemEntity.Hash = hash;

            await ih.AddItemHistory(itemHistory, ActionType.Restocked);
            var restocked = PropCopier.Copy(itemEntity, new ResponseItemModel());
            log.LogInformation("Restocked item with ID {ItemId}.", itemEntity.Id);
            return (restocked, null);
        }
    }
}