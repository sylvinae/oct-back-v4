using API.Db;
using API.Interfaces;
using API.Interfaces.Item;
using API.Models.Item;
using API.Utils;
using API.Validators;
using FluentValidation;

namespace API.Services.Item;

public class UpdateItemService(
    Context _db,
    ILogger<UpdateItemService> _log,
    IItemHistoryService _ih,
    IValidator<UpdateItemModel> _updateValidator
) : IUpdateItemService
{
    public async Task<(
        List<FailedResponseItemModel> failed,
        List<ResponseItemModel> updated
    )> UpdateItems(List<UpdateItemModel> items)
    {
        var (updated, failed) = (
            new List<ResponseItemModel>(),
            new List<FailedResponseItemModel>()
        );

        if (items == null || items.Count == 0)
        {
            _log.LogInformation("No items to update.");
            return (failed, updated);
        }

        try
        {
            foreach (var item in items)
            {
                if (!await ValidateItem(item, failed))
                    continue;

                if (!await ProcessUpdate(item, updated, failed))
                    continue;
            }

            await _db.SaveChangesAsync();
            _log.LogInformation("Successfully saved changes to DB.");
            return (failed, updated);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while updating items.");
            return (failed, updated);
        }
    }

    private async Task<bool> ValidateItem(
        UpdateItemModel item,
        List<FailedResponseItemModel> failed
    )
    {
        (bool isValid, string? error) = await SuperValidator.Check(_updateValidator, item);

        if (!isValid && error != null)
        {
            _log.LogWarning("Model state invalid. {x}", error);
            failed.Add(PropCopier.Copy(item, new FailedResponseItemModel { Error = error }));
            return false;
        }
        return true;
    }

    private async Task<bool> ProcessUpdate(
        UpdateItemModel item,
        List<ResponseItemModel> updated,
        List<FailedResponseItemModel> failed
    )
    {
        var existingItem = await _db.Items.FindAsync(item.Id);
        if (existingItem == null)
        {
            AddFailedItem(failed, item, $"Item with ID {item.Id} not found.");
            return false;
        }

        var newHash = Cryptics.ComputeHash(item);
        if (existingItem.Hash == newHash)
        {
            AddFailedItem(failed, item, $"Item with ID {item.Id} has no changes. Skipped.");
            return false;
        }

        try
        {
            _log.LogInformation("Updating item with ID {ItemId}.", existingItem.Id);

            item.Hash = newHash;
            _db.Entry(existingItem).CurrentValues.SetValues(item);

            updated.Add(PropCopier.Copy(existingItem, new ResponseItemModel()));

            await _ih.AddItemHistory(
                PropCopier.Copy(item, new AddItemHistoryModel { ItemId = item.Id, Hash = newHash }),
                ActionType.Updated
            );
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to update item with ID {ItemId}.", item.Id);
            AddFailedItem(failed, item, ex.Message);
            return false;
        }
    }

    private static void AddFailedItem(
        List<FailedResponseItemModel> failed,
        UpdateItemModel item,
        string error
    )
    {
        failed.Add(
            PropCopier.Copy(item, new FailedResponseItemModel { Error = error, ItemHistory = null })
        );
    }
}
