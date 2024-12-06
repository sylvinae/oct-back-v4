using API.Db;
using API.Entities.User;
using API.Interfaces;
using API.Item.ItemHistory;
using API.Models.Item;
using API.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Services;

public class ItemHistoryService(
    Context _db,
    ILogger<ItemHistoryService> _log,
    UserManager<UserEntity> _userManager,
    IHttpContextAccessor _httpContextAccessor
) : IItemHistoryService
{
    public async Task<bool> AddItemHistory(AdddItemHistoryModel itemHistory, ActionType action)
    {
        try
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);

            if (user != null)
            {
                var itemHistoryEntity = PropCopier.Copy(
                    itemHistory,
                    new ItemHistoryEntity { ItemId = itemHistory.ItemId, UserId = user.Id }
                );
                itemHistoryEntity.Action = action.ToString();
                var result = await _db.ItemHistories.AddAsync(itemHistoryEntity);
                _log.LogInformation(
                    "Added {action} history to item {id}",
                    action.ToString(),
                    itemHistory.ItemId
                );
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public async Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid itemId)
    {
        try
        {
            var response = new List<ResponseItemHistoryModel>();

            var result = await _db.ItemHistories.Where(ih => ih.ItemId == itemId).ToListAsync();

            foreach (var item in result)
            {
                response.Add(PropCopier.Copy(item, new ResponseItemHistoryModel()));
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching item history", ex);
        }
    }
}
