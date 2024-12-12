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

public class CreateItemService(
    Context db,
    ILogger<CreateItemService> log,
    IValidator<CreateItemModel> createValidator,
    IItemHistoryService ih
) : ICreateItemService
{
    public async Task<(
        List<FailedResponseItemModel> failed,
        List<ResponseItemModel> created
        )> CreateItems(List<CreateItemModel> items)
    {
        log.LogInformation("Create Items called.");

        var failed = new List<FailedResponseItemModel>();
        var created = new List<ResponseItemModel>();

        if (items.Count == 0)
        {
            log.LogInformation("No items to create.");
            return (failed, created);
        }

        foreach (var item in items)
            try
            {
                var (isValid, validationError) = await ValidateItem(item);
                if (!isValid)
                {
                    AddToFailedList(item, validationError ?? "", failed);
                    continue;
                }

                var existingItem = await CheckForExistingItem(item);
                if (existingItem != null)
                {
                    AddToFailedList(item, "Item already exists.", failed);
                    continue;
                }

                var createdItem = await CreateNewItem(item);
                created.Add(createdItem);
            }
            catch (Exception ex)
            {
                log.LogError(
                    ex,
                    "An error occurred while creating item {Brand} - {Generic}.",
                    item.Brand,
                    item.Generic
                );
                AddToFailedList(item, ex.Message, failed);
            }

        await db.SaveChangesAsync();
        log.LogInformation("Finished creating items. Exiting.");
        return (failed, created);
    }

    private async Task<(bool isValid, string? error)> ValidateItem(CreateItemModel item)
    {
        log.LogInformation("Validating item {Brand} - {Generic}.", item.Brand, item.Generic);
        return await SuperValidator.Check(createValidator, item);
    }

    private async Task<ItemEntity?> CheckForExistingItem(CreateItemModel item)
    {
        var hash = Cryptics.ComputeHash(item);
        log.LogInformation("Checking for existing item with hash {Hash}.", hash);
        return await db.Items.FirstOrDefaultAsync(i => i.Hash == hash);
    }

    private void AddToFailedList(
        CreateItemModel item,
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

    private async Task<ResponseItemModel> CreateNewItem(CreateItemModel item)
    {
        var hash = Cryptics.ComputeHash(item);

        var itemEntity = PropCopier.Copy(
            item,
            new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold }
        );

        var itemHistory = PropCopier.Copy(
            item,
            new AddItemHistoryModel { ItemId = itemEntity.Id, Hash = hash }
        );

        await db.Items.AddAsync(itemEntity);
        await ih.AddItemHistory(itemHistory, ActionType.Created);


        log.LogInformation("Created item with ID {ItemId}.", itemEntity.Id);
        return PropCopier.Copy(itemEntity, new ResponseItemModel());
    }
}