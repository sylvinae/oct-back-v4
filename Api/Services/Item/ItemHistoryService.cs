using API.Db;
using API.Entities.Item;
using API.Entities.User;
using API.Models.Item;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Item;

public class ItemHistoryService(
    Context db,
    ILogger<ItemHistoryService> log,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor
) : IItemHistoryService
{
    public async Task<bool> AddItemHistory(AddItemHistoryModel itemHistory, ActionType action)
    {
        try
        {
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);

            var itemHistoryEntity = PropCopier.Copy(
                itemHistory,
                new ItemHistoryEntity { ItemId = itemHistory.ItemId, UserId = user!.Id }
            );
            itemHistoryEntity.Action = action.ToString();
            await db.ItemHistories.AddAsync(itemHistoryEntity);
            log.LogInformation(
                "Added {action} history to item {id}",
                action.ToString(),
                itemHistory.ItemId
            );

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error adding history. {x}", ex);
            return false;
        }
    }

    public async Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid itemId)
    {
        var result = await db.ItemHistories.Where(ih => ih.ItemId == itemId).ToListAsync();
        return result.Select(item => PropCopier.Copy(item, new ResponseItemHistoryModel())).ToList();
    }

    public async Task<bool> AddItemHistoryRange(List<AddItemHistoryModel> itemHistory, ActionType action)
    {
        try
        {
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
            var entities = itemHistory.Select(item => PropCopier.Copy(itemHistory,
                new ItemHistoryEntity
                    { ItemId = item.ItemId, UserId = user!.Id, Action = action.ToString() })).ToList();

            await db.ItemHistories.AddRangeAsync(entities);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}