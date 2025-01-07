using API.Db;
using API.Entities.Item;
using API.Entities.User;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.AspNetCore.Identity;

namespace API.Services.Item;

public class ItemHistoryService(
    Context db,
    ILogger<ItemHistoryService> log,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor
) : IItemHistoryService
{
    public async Task<bool> AddItemHistory(CreateItemHistoryModel itemHistory)
    {
        try
        {
            var entities = CreateItemHistoryEntities([itemHistory]);

            await db.ProductHistories.AddAsync(entities.First());
            log.LogInformation(
                "Added {action} history to item {id}", itemHistory.Action, itemHistory.ItemId
            );

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error adding history. {x}", ex);
            return false;
        }
    }

    public async Task<bool> AddItemHistoryRange(List<CreateItemHistoryModel> itemHistory)
    {
        try
        {
            var entities = CreateItemHistoryEntities(itemHistory);

            await db.ProductHistories.AddRangeAsync(entities);
            log.LogInformation("Added item history for {count} items.", itemHistory.Count);

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error adding item history range. {x}", ex);
            return false;
        }
    }

    private List<ItemHistoryEntity> CreateItemHistoryEntities(List<CreateItemHistoryModel> itemHistory)
    {
        var user = userManager.GetUserAsync(httpContextAccessor.HttpContext!.User).Result;

        return itemHistory.Select(item =>
        {
            var entity = new ItemHistoryEntity
            {
                ProductId = item.ItemId,
                UserId = user!.Id,
                Hash = item.Hash,
                Action = item.Action!
            };

            return PropCopier.Copy(item, entity);
        }).ToList();
    }
}