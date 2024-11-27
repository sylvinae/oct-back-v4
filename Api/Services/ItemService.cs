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

    public async Task<(
        List<ResponseItemModel> failed,
        List<ResponseItemModel> created
    )> CreateItems(List<CreateItemModel> items)
    {
        var (created, failed) = (new List<ResponseItemModel>(), new List<ResponseItemModel>());
        foreach (var item in items)
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
                    failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
                    continue;
                }
                if (!item.HasExpiry)
                    item.Expiry = null;
                else if (item.Expiry != null)
                {
                    _ = DateTime.TryParse(item.Expiry, out DateTime parsedDate);
                    DateTime currentDate = DateTime.Today;
                    item.IsExpired = DateTime.Compare(parsedDate, currentDate) <= 0;
                }

                item.IsLow = item.Stock <= item.LowThreshold;
                var itemEntity = PropCopier.Copy(item, new ItemEntity { Hash = hash });
                var result = await _db.Items.AddAsync(itemEntity);

                await _ih.AddItemHistory(
                    PropCopier.Copy(
                        result.Entity,
                        new AdddItemHistoryModel { ItemId = result.Entity.Id }
                    ),
                    ActionType.Created
                );

                created.Add(PropCopier.Copy(item, new ResponseItemModel()));
                _log.LogInformation("Created item {ItemId}.", result.Entity.Id);
            }
            catch (Exception ex)
            {
                failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
                _log.LogError(
                    ex,
                    "Failed to create item {Brand} - {Generic}.",
                    item.Brand,
                    item.Generic
                );
            }
        }
        await _db.SaveChangesAsync();
        _log.LogInformation("Finished processing all items.");
        return (failed, created);
    }

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
        _log.LogDebug("Starting update for {Count} item(s).", items.Count);
        var (updated, failed) = (new List<ResponseItemModel>(), new List<ResponseItemModel>());
        foreach (var item in items)
        {
            try
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
            catch (DbUpdateConcurrencyException ex)
            {
                _log.LogError(ex, "Concurrency conflict for item {ItemId}.", item.Id);
                failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to update item with ID {ItemId}.", item.Id);
                failed.Add(PropCopier.Copy(item, new ResponseItemModel()));
            }
        }
        if (updated.Count != 0)
        {
            try
            {
                await _db.SaveChangesAsync();
                _log.LogInformation("Successfully saved changes to DB.");
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "Failed to save updates to database.");
                failed.AddRange(updated.Select(u => new ResponseItemModel()));
            }
        }
        return (failed, updated);
    }

    public async Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds)
    {
        var failed = new List<Guid>();
        var deletedItems = new List<Guid>();
        try
        {
            foreach (var id in itemIds)
            {
                try
                {
                    var item = await _db.Items.FindAsync(id);
                    if (item == null)
                    {
                        _log.LogWarning("{ItemId} not found.", id);
                        failed.Add(id);
                        continue;
                    }

                    item.IsDeleted = true;
                    deletedItems.Add(id);

                    await _ih.AddItemHistory(
                        PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                        ActionType.Deleted
                    );
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error processing item {ItemId}", id);
                    failed.Add(id);
                }
            }
            if (deletedItems.Count != 0)
                await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency error during bulk delete");
            failed.AddRange(itemIds);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error during bulk delete");
            failed.AddRange(itemIds);
        }
        return (failed, deletedItems);
    }

    public async Task<(List<Guid> failed, List<Guid> undeleted)> UndeleteItems(List<Guid> itemIds)
    {
        var failed = new List<Guid>();
        var undeleted = new List<Guid>();
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var id in itemIds)
            {
                try
                {
                    var item = await _db.Items.FindAsync(id);
                    if (item == null)
                    {
                        _log.LogWarning("{ItemId} not found.", id);
                        failed.Add(id);
                        continue;
                    }
                    item.IsDeleted = false;
                    undeleted.Add(id);
                    _log.LogInformation("Undeleted {ItemId}.", id);

                    await _ih.AddItemHistory(
                        PropCopier.Copy(item, new AdddItemHistoryModel { ItemId = item.Id }),
                        ActionType.Deleted
                    );
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _log.LogError(ex, "Concurrency conflict for item {ItemId}.", id);
                    failed.Add(id);
                }
            }
            if (undeleted.Count != 0)
                await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while undeleting items.");
            failed.AddRange(itemIds);
            await transaction.RollbackAsync();
        }
        return (failed, undeleted);
    }

    public async Task<(List<ResponseItemModel> items, int count)> SearchItems(
        string query,
        int page,
        int limit,
        bool includeHistory
    )
    {
        try
        {
            var baseQuery = _db.Items.Where(i =>
                (i.Brand ?? "").Contains(query, StringComparison.CurrentCultureIgnoreCase)
                && !i.IsDeleted
            );

            if (includeHistory)
            {
                baseQuery = baseQuery.Include(i => i.ItemHistory);
            }

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
