using System.Linq;
using API.Interfaces;
using Api.Interfaces;
using API.Models.Item;
using Api.Services;
using API.Utils;
using Data.Db;
using Data.Entities.Item;
using Data.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class ItemService(
    Context db,
    ILogger<ItemService> log,
    UserManager<UserEntity> userManager,
    IItemHistoryService ih,
    IHttpContextAccessor httpContextAccessor
) : IItemService
{
    private readonly Context _db = db;
    private readonly ILogger<IItemService> _log = log;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IItemHistoryService _ih = ih;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    //create items
    public async Task<ResponseItemModel?> CreateItem(CreateItemModel item)
    {
        try
        {
            var hash = Cryptics.ComputeHash(item);
            var existingItem = await _db.Items.FirstOrDefaultAsync(i => i.Hash == hash);

            if (existingItem != null)
            {
                _log.LogWarning(
                    "Item already exists: {Brand} - {Generic}.",
                    item.Brand,
                    item.Generic
                );
                return null;
            }

            // Expiry handling
            if (!item.HasExpiry)
                item.Expiry = null;
            else if (item.Expiry != null)
            {
                if (DateTime.TryParse(item.Expiry, out DateTime parsedDate))
                {
                    DateTime currentDate = DateTime.Today;
                    item.IsExpired = DateTime.Compare(parsedDate, currentDate) <= 0;

                    // Format the date to specific format
                    item.Expiry = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    _log.LogWarning(
                        "Invalid date format for item: {Brand} - {Generic}.",
                        item.Brand,
                        item.Generic
                    );
                    return null;
                }
            }

            // Low stock calculation
            item.IsLow = item.Stock <= item.LowThreshold;

            // Create entity
            var itemEntity = PropCopier.Copy(item, new ItemEntity { Hash = hash });
            var result = await _db.Items.AddAsync(itemEntity);

            // Add item history
            await _ih.AddItemHistory(
                PropCopier.Copy(
                    result.Entity,
                    new AdddItemHistoryModel { ItemId = result.Entity.Id }
                ),
                ActionType.Created
            );

            _log.LogInformation("Created item {ItemId}.", result.Entity.Id);

            return PropCopier.Copy(item, new ResponseItemModel());
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                "Failed to create item {Brand} - {Generic}.",
                item.Brand,
                item.Generic
            );

            return null;
        }
    }

    public async Task<(
        List<ResponseItemModel> failed,
        List<ResponseItemModel> created
    )> CreateItems(List<CreateItemModel> items)
    {
        var (created, failed) = (new List<ResponseItemModel>(), new List<ResponseItemModel>());

        foreach (var item in items)
        {
            var result = await CreateItem(item);

            if (result != null)
                created.Add(result);
            else
                failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
        }

        await _db.SaveChangesAsync();
        _log.LogInformation("Finished processing all items.");
        return (failed, created);
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
        List<ResponseItemModel> failed,
        List<ResponseItemModel> updated
    )> UpdateItems(List<UpdateItemModel> items)
    {
        var (updated, failed) = (new List<ResponseItemModel>(), new List<ResponseItemModel>());

        if (items == null || !items.Any())
        {
            _log.LogInformation("No items to update.");
            return (failed, updated);
        }

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                var existingItem = await _db.Items.FindAsync(item.Id);
                if (existingItem == null)
                {
                    _log.LogWarning("Item with ID {ItemId} not found.", item.Id);
                    failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
                    continue;
                }

                var newHash = Cryptics.ComputeHash(item);
                if (existingItem.Hash == newHash)
                {
                    _log.LogInformation("Item with ID {ItemId} has no changes. Skipped.", item.Id);
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
            }

            if (updated.Any())
            {
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

    public async Task<ResponseItemModel?> UpdateItem(UpdateItemModel item)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        var newHash = "";
        try
        {
            var existingItem = await _db.Items.FindAsync(item.Id);
            if (existingItem == null)
            {
                _log.LogWarning("Item with ID {ItemId} not found.", item.Id);
                return null;
            }

            newHash = Cryptics.ComputeHash(item);
            if (existingItem.Hash == newHash)
            {
                _log.LogInformation("Item with ID {ItemId} has no changes.", item.Id);
                return null;
            }

            _log.LogInformation("Updating item with ID {ItemId}.", existingItem.Id);

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
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency conflict for item {ItemId}.", item.Id);
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to update item with ID {ItemId}.", item.Id);
            await transaction.RollbackAsync();
        }

        return PropCopier.Copy(item, new ResponseItemModel());
    }

    //delete items
    public async Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds)
    {
        var (deleted, failed) = (new List<Guid>(), new List<Guid>());

        if (itemIds == null || !itemIds.Any())
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
                    PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                    ActionType.Deleted
                );
            }

            if (deleted.Any())
            {
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _log.LogInformation("Successfully deleted items and added history.");
            }

            return (failed, deleted);
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

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var items = await _db.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();

            foreach (var item in items)
            {
                try
                {
                    if (item.IsDeleted == false)
                    {
                        _log.LogInformation("Item {ItemId} is already restored.", item.Id);
                        continue;
                    }

                    item.IsDeleted = false;
                    restored.Add(item.Id);

                    _log.LogInformation("Restoring item {ItemId}.", item.Id);

                    await _ih.AddItemHistory(
                        PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                        ActionType.Restored
                    );
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _log.LogError(ex, "Concurrency conflict for item {ItemId}.", item.Id);
                    failed.Add(item.Id);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error while restoring item {ItemId}.", item.Id);
                    failed.Add(item.Id);
                }
            }

            if (restored.Any())
            {
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _log.LogInformation("Successfully restored items.");
            }

            return (failed, restored);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while restoring items.");
            await transaction.RollbackAsync();
            failed.AddRange(itemIds);
            return (failed, restored);
        }
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

            _log.LogInformation("Successfully restored item {ItemId}.", itemId);
            return itemId;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency conflict for item {ItemId}.", itemId);
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error restoring item {ItemId}.", itemId);
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

            //1 loop based idk looks simpler
            // List<ResponseItemModel> respo = [];
            // foreach (var item in items)
            // {
            //     List<ResponseItemHistoryModel> history = [];
            //     foreach (var h in item.ItemHistory)
            //     {
            //         history.Add(
            //             PropCopier.Copy(
            //                 h,
            //                 new ResponseItemHistoryModel
            //                 {
            //                     Id = h.Id,
            //                     UserId = h.UserId,
            //                     ItemId = h.ItemId,
            //                 }
            //             )
            //         );
            //     }

            //     respo.Add(
            //         PropCopier.Copy(
            //             item,
            //             new ResponseItemModel
            //             {
            //                 Id = item.Id,
            //                 ItemHistory = includeHistory ? history : null,
            //             }
            //         )
            //     );
            // }

            //2 linq based, scarier but fancier
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
