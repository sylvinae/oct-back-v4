using System.Resources;
using API.Interfaces;
using API.Models.Item;
using API.Utils;
using API.Validators;
using Data.Db;
using Data.Entities.Item;
using Data.Entities.User;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// using Sprache;

namespace API.Services;

public class ItemService(
    Context db,
    ILogger<ItemService> log,
    UserManager<UserEntity> userManager,
    IItemHistoryService ih,
    IHttpContextAccessor httpContextAccessor,
    IValidator<CreateItemModel> createValidator,
    IValidator<UpdateItemModel> updateValidator
) : IItemService
{
    private readonly Context _db = db;
    private readonly ILogger<IItemService> _log = log;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IItemHistoryService _ih = ih;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IValidator<CreateItemModel> _createValidator = createValidator;
    private readonly IValidator<UpdateItemModel> _updateValidator = updateValidator;

    //create items
    //model validation done in endpoint
    public async Task<(FailedResponseItemModel? failed, ResponseItemModel? created)> CreateItem(
        CreateItemModel item
    )
    {
        _log.LogDebug("Create Item called.");
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var hash = Cryptics.ComputeHash(item);

            var existingItem = await _db.Items.FirstOrDefaultAsync(i => i.Hash == hash);
            if (existingItem != null)
            {
                _log.LogWarning(
                    "Item already exists: {Brand} - {Generic}",
                    item.Brand,
                    item.Generic
                );

                var failedResponse = PropCopier.Copy(
                    existingItem,
                    new FailedResponseItemModel { Error = "Item already exists." }
                );
                return (failedResponse, null);
            }

            var itemEntity = PropCopier.Copy(
                item,
                new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold }
            );

            var result = await _db.Items.AddAsync(itemEntity);

            await _ih.AddItemHistory(
                PropCopier.Copy(
                    item,
                    new AdddItemHistoryModel { ItemId = result.Entity.Id, Hash = hash }
                ),
                ActionType.Created
            );

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _log.LogInformation("Successfully created item {ItemId}. Exiting.", result.Entity.Id);

            var createdResponse = PropCopier.Copy(
                result.Entity,
                new ResponseItemModel { ItemHistory = null }
            );

            return (null, createdResponse);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _log.LogError(
                ex,
                "Failed to create item {Brand} - {Generic}. Rolling back changes.",
                item.Brand,
                item.Generic
            );

            var failedResponse = PropCopier.Copy(
                item,
                new FailedResponseItemModel
                {
                    Error = "An unexpected error occurred. Please try again later.",
                }
            );
            _log.LogInformation("Exiting due to exceptions.");

            return (failedResponse, null);
        }
    }

    //create items
    //manual model validation
    public async Task<(
        List<FailedResponseItemModel> failed,
        List<ResponseItemModel> created
    )> CreateItems(List<CreateItemModel> items)
    {
        _log.LogInformation("Create Items called.");
        var failed = new List<FailedResponseItemModel>();
        var created = new List<ResponseItemModel>();

        if (items == null || items.Count == 0)
        {
            _log.LogInformation("No items to create.");
            return (failed, created);
        }

        try
        {
            foreach (var item in items)
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    (bool isValid, string? error) = await SuperValidator.Check(
                        _createValidator,
                        item
                    );

                    if (!isValid && error != null)
                    {
                        _log.LogWarning("Model state invalid. {x}", error);
                        failed.Add(
                            PropCopier.Copy(item, new FailedResponseItemModel { Error = error })
                        );
                        continue;
                    }

                    var hash = Cryptics.ComputeHash(item);
                    var existingItem = await _db.Items.FirstOrDefaultAsync(i => i.Hash == hash);

                    if (existingItem != null)
                    {
                        _log.LogWarning(
                            "Item already exists: {Brand} - {Generic}.",
                            item.Brand,
                            item.Generic
                        );
                        failed.Add(PropCopier.Copy(existingItem, new FailedResponseItemModel()));
                        continue;
                    }

                    var itemEntity = PropCopier.Copy(
                        item,
                        new ItemEntity { Hash = hash, IsLow = item.Stock <= item.LowThreshold }
                    );

                    var result = await _db.Items.AddAsync(itemEntity);

                    await _ih.AddItemHistory(
                        PropCopier.Copy(
                            item,
                            new AdddItemHistoryModel { ItemId = result.Entity.Id, Hash = hash }
                        ),
                        ActionType.Created
                    );

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    created.Add(
                        PropCopier.Copy(itemEntity, new ResponseItemModel { ItemHistory = null })
                    );

                    _log.LogInformation("Created item with ID {ItemId}.", itemEntity.Id);
                }
                catch (Exception ex)
                {
                    _log.LogError(
                        ex,
                        "An error occurred while creating item {Brand} - {Generic}.",
                        item.Brand,
                        item.Generic
                    );

                    await transaction.RollbackAsync();
                }
            }

            _log.LogInformation("Finished creating items. Exiting.");
            return (failed, created);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An unexpected error occurred while processing items.");
            return (failed, created);
        }
    }

    //get items
    public async Task<(List<ResponseItemModel> items, int totalCount)> GetItems(
        int page,
        int limit,
        bool includeHistory
    )
    {
        try
        {
            _log.LogDebug("Service called. Getting some items...");
            var query = _db.Items.Where(i => !i.IsDeleted);
            if (includeHistory)
                query = query.Include(i => i.ItemHistory);
            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return (Array.Empty<ResponseItemModel>().ToList(), 0);
            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
            var response = items
                .Select(item =>
                    PropCopier.Copy(
                        item,
                        new ResponseItemModel
                        {
                            ItemHistory = includeHistory
                                ? item
                                    .ItemHistory?.Select(h =>
                                        PropCopier.Copy(
                                            h,
                                            new ResponseItemHistoryModel
                                            {
                                                UserId = h.UserId,
                                                ItemId = h.ItemId,
                                            }
                                        )
                                    )
                                    .ToList()
                                : null,
                        }
                    )
                )
                .ToList();
            return (response, totalCount);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while fetching items.");
            throw;
        }
    }

    public async Task<ResponseItemModel?> GetItem(Guid id, bool includeHistory)
    {
        try
        {
            _log.LogDebug("Service called. Fetching item with ID {ItemId}...", id);
            var query = _db.Items.Where(i => !i.IsDeleted && i.Id == id);
            if (includeHistory)
                query = query.Include(i => i.ItemHistory);
            var item = await query.FirstOrDefaultAsync();
            if (item == null)
                return null;
            var response = PropCopier.Copy(
                item,
                new ResponseItemModel
                {
                    ItemHistory = includeHistory
                        ? item
                            .ItemHistory?.Select(h =>
                                PropCopier.Copy(
                                    h,
                                    new ResponseItemHistoryModel
                                    {
                                        UserId = h.UserId,
                                        ItemId = h.ItemId,
                                    }
                                )
                            )
                            .ToList()
                        : null,
                }
            );
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while fetching item with ID {ItemId}.", id);
            throw;
        }
    }

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

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                (bool isValid, string? error) = await SuperValidator.Check(_updateValidator, item);

                if (!isValid && error != null)
                {
                    _log.LogWarning("Model state invalid. {x}", error);
                    failed.Add(
                        PropCopier.Copy(item, new FailedResponseItemModel { Error = error })
                    );
                    continue;
                }

                var existingItem = await _db.Items.FindAsync(item.Id);
                if (existingItem == null)
                {
                    _log.LogWarning("Item with ID {ItemId} not found.", item.Id);
                    failed.Add(
                        PropCopier.Copy(
                            item,
                            new FailedResponseItemModel
                            {
                                Error = $"Item with ID {item.Id} not found.",
                            }
                        )
                    );
                    continue;
                }

                var newHash = Cryptics.ComputeHash(item);
                if (existingItem.Hash == newHash)
                {
                    _log.LogInformation("Item with ID {ItemId} has no changes. Skipped.", item.Id);
                    failed.Add(
                        PropCopier.Copy(
                            item,
                            new FailedResponseItemModel
                            {
                                Error = $"Item with ID {item.Id} has no changes.Skipped.",
                            }
                        )
                    );
                    continue;
                }

                _log.LogInformation("Updating item with ID {ItemId}.", existingItem.Id);

                item.Hash = newHash;
                _db.Entry(existingItem).CurrentValues.SetValues(item);

                updated.Add(PropCopier.Copy(existingItem, new ResponseItemModel()));

                await _ih.AddItemHistory(
                    PropCopier.Copy(
                        item,
                        new AdddItemHistoryModel { ItemId = item.Id, Hash = newHash }
                    ),
                    ActionType.Updated
                );
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                _log.LogInformation("Successfully saved changes to DB.");
            }

            return (failed, updated);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while updating items.");
            await transaction.RollbackAsync();
            return (failed, updated);
        }
    }

    public async Task<(FailedResponseItemModel? failed, ResponseItemModel? created)> UpdateItem(
        UpdateItemModel item
    )
    {
        try
        {
            var existingItem = await _db.Items.FindAsync(item.Id);
            if (existingItem == null)
            {
                _log.LogWarning("Item with ID {ItemId} not found.", item.Id);
                return (
                    PropCopier.Copy(
                        item,
                        new FailedResponseItemModel { Error = $"Item with ID {item.Id} not found." }
                    ),
                    null
                );
            }

            string? newHash = Cryptics.ComputeHash(item);
            if (existingItem.Hash == newHash)
            {
                _log.LogInformation("Item with ID {ItemId} has no changes.", item.Id);
                return (
                    PropCopier.Copy(
                        item,
                        new FailedResponseItemModel
                        {
                            Error = $"Item with ID {item.Id} has no changes.",
                        }
                    ),
                    null
                );
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                item.Hash = newHash;
                _db.Entry(existingItem).CurrentValues.SetValues(item);

                await _ih.AddItemHistory(
                    PropCopier.Copy(
                        item,
                        new AdddItemHistoryModel { ItemId = item.Id, Hash = newHash }
                    ),
                    ActionType.Updated
                );

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _log.LogInformation(
                    "Successfully updated item and added item history for item {ItemId}.",
                    item.Id
                );

                return (null, PropCopier.Copy(existingItem, new ResponseItemModel()));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to update item with ID {ItemId}.", item.Id);
                await transaction.RollbackAsync();
                return (
                    PropCopier.Copy(
                        item,
                        new FailedResponseItemModel
                        {
                            Error = "Failed to update item due to an error.",
                        }
                    ),
                    null
                );
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error occurred while processing item {ItemId}.", item.Id);
            return (
                PropCopier.Copy(
                    item,
                    new FailedResponseItemModel { Error = "An unexpected error occurred." }
                ),
                null
            );
        }
    }

    //delete items
    public async Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds)
    {
        var (deleted, failed) = (new List<Guid>(), new List<Guid>());

        if (itemIds == null || itemIds.Count == 0)
        {
            _log.LogInformation("No items to delete.");
            return (failed, deleted);
        }

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            foreach (var id in itemIds)
            {
                var item = await _db.Items.FindAsync(id);
                if (item == null)
                {
                    _log.LogWarning("Item with ID {ItemId} not found.", id);
                    failed.Add(id);
                    continue;
                }

                item.IsDeleted = true;
                deleted.Add(id);

                await _ih.AddItemHistory(
                    PropCopier.Copy(
                        item,
                        new AdddItemHistoryModel { ItemId = item.Id, Hash = item.Hash }
                    ),
                    ActionType.Updated
                );
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _log.LogInformation(
                "Successfully deleted {DeletedCount} items and added history for {DeletedCount} items.",
                deleted.Count,
                deleted.Count
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency error during bulk delete");
            await transaction.RollbackAsync();
            failed.AddRange(itemIds);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error during bulk delete");
            await transaction.RollbackAsync();
            failed.AddRange(itemIds);
        }

        return (failed, deleted);
    }

    public async Task<Guid?> DeleteItem(Guid itemId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var item = await _db.Items.FindAsync(itemId);
            if (item == null)
            {
                _log.LogWarning("Item with ID {ItemId} not found.", itemId);
                return null;
            }

            item.IsDeleted = true;

            await _ih.AddItemHistory(
                PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                ActionType.Deleted
            );

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            _log.LogInformation(
                "Successfully deleted item and added history for item {ItemId}.",
                itemId
            );

            return itemId;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency error while deleting item with ID {ItemId}.", itemId);
            await transaction.RollbackAsync();
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                "An unexpected error occurred while deleting item with ID {ItemId}.",
                itemId
            );
            await transaction.RollbackAsync();
            return null;
        }
    }

    public async Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds)
    {
        var failed = new List<Guid>();
        var restored = new List<Guid>();

        foreach (var itemId in itemIds)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var item = await _db.Items.FindAsync(itemId);
                if (item == null)
                {
                    _log.LogWarning("Item with ID {ItemId} not found.", itemId);
                    failed.Add(itemId);
                    continue;
                }

                if (item.IsDeleted == false)
                {
                    _log.LogInformation("Item {ItemId} is already restored.", itemId);
                    continue;
                }

                item.IsDeleted = false;
                restored.Add(itemId);

                _log.LogInformation("Restoring item {ItemId}.", itemId);

                await _ih.AddItemHistory(
                    PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                    ActionType.Restored
                );

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _log.LogInformation("Successfully restored item with ID {ItemId}.", itemId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _log.LogError(ex, "Concurrency conflict for item {ItemId}.", itemId);
                await transaction.RollbackAsync();
                failed.Add(itemId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error while restoring item with ID {ItemId}.", itemId);
                await transaction.RollbackAsync();
                failed.Add(itemId);
            }
        }

        return (failed, restored);
    }

    public async Task<Guid?> RestoreItem(Guid itemId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var item = await _db.Items.FindAsync(itemId);
            if (item == null)
            {
                _log.LogWarning("Item with ID {ItemId} not found.", itemId);
                return null;
            }

            if (item.IsDeleted == false)
            {
                _log.LogInformation("Item with ID {ItemId} is already restored.", itemId);
                return itemId;
            }

            item.IsDeleted = false;

            await _ih.AddItemHistory(
                PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                ActionType.Restored
            );

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            _log.LogInformation("Successfully restored item with ID {ItemId}.", itemId);
            return itemId;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(
                ex,
                "Concurrency conflict while restoring item with ID {ItemId}.",
                itemId
            );
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while restoring item with ID {ItemId}.", itemId);
            await transaction.RollbackAsync();
        }

        return null;
    }

    public async Task<(List<ResponseItemModel> items, int count)> SearchItems(
        string query,
        int page,
        int limit,
        bool isdeleted,
        bool isExpired,
        bool isReagent,
        bool isLow,
        bool includeHistory,
        bool? hasExpiry
    )
    {
        try
        {
            var baseQuery = _db.Items.Where(i => !i.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                baseQuery = baseQuery.Where(i =>
                    (i.Barcode ?? "").ToLower().Contains(query)
                    || (i.Brand ?? "").ToLower().Contains(query)
                    || (i.Generic ?? "").ToLower().Contains(query)
                    || (i.Classification ?? "").ToLower().Contains(query)
                    || (i.Formulation ?? "").ToLower().Contains(query)
                    || (i.Location ?? "").ToLower().Contains(query)
                    || (i.Company ?? "").ToLower().Contains(query)
                    || (i.Wholesale.ToString() ?? "").ToLower().Contains(query)
                    || (i.Retail.ToString() ?? "").ToLower().Contains(query)
                );
            }
            baseQuery = isExpired ? baseQuery.Where(i => i.IsExpired) : baseQuery;
            baseQuery = isReagent ? baseQuery.Where(i => i.IsExpired) : baseQuery;
            baseQuery = isLow ? baseQuery.Where(i => i.IsExpired) : baseQuery;
            baseQuery = includeHistory ? baseQuery.Include(i => i.ItemHistory) : baseQuery;

            baseQuery = hasExpiry.HasValue
                ? baseQuery.Where(i => i.IsExpired == hasExpiry.Value)
                : baseQuery;

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(i => i.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var response = items
                .Select(item =>
                    PropCopier.Copy(
                        item,
                        new ResponseItemModel
                        {
                            Id = item.Id,
                            ItemHistory = includeHistory
                                ? item
                                    .ItemHistory?.Select(h =>
                                        PropCopier.Copy(
                                            h,
                                            new ResponseItemHistoryModel
                                            {
                                                Id = h.Id,
                                                UserId = h.UserId,
                                                ItemId = h.ItemId,
                                            }
                                        )
                                    )
                                    .ToList()
                                : null,
                        }
                    )
                )
                .ToList();

            return (response, totalCount);
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                "An error occurred while searching items. Query: {Query}, Page: {Page}, Limit: {Limit}, IncludeHistory: {IncludeHistory}",
                query,
                page,
                limit,
                includeHistory
            );
            throw;
        }
    }
}
