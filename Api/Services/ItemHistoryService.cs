using API.Db;
using API.Entities.Item;
using API.Entities.User;
using API.Interfaces;
using API.Models.Item;
using API.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

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
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User!);

            if (user != null)
            {
                var itemHistoryEntity = PropCopier.Copy(
                    itemHistory,
                    new ItemHistoryEntity { ItemId = itemHistory.ItemId, UserId = user.Id }
                );
                itemHistoryEntity.Action = action.ToString();
                await db.ItemHistories.AddAsync(itemHistoryEntity);
                log.LogInformation(
                    "Added {action} history to item {id}",
                    action.ToString(),
                    itemHistory.ItemId
                );
            }

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
        try
        {
            var response = new List<ResponseItemHistoryModel>();

            var result = await db.ItemHistories.Where(ih => ih.ItemId == itemId).ToListAsync();

            foreach (var item in result) response.Add(PropCopier.Copy(item, new ResponseItemHistoryModel()));

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching item history", ex);
        }
    }
}