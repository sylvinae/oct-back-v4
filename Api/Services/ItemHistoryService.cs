using Api.Interfaces;
using API.Models.Item;
using API.Utils;
using Data.Db;
using Data.Entities.User;
using Data.Item.ItemHistory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace Api.Services;

public class ItemHistoryService(
    Context db,
    ILogger<ItemHistoryService> log,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor
) : IItemHistoryService
{
    private readonly Context _db = db;
    private readonly ILogger<ItemHistoryService> _log = log;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<bool> AddItemHistory(AdddItemHistoryModel itemHistory, ActionType action)
    {
        try
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);

            if (user != null)
            {
                var itemHistoryEntity = PropCopier.Copy(
                    itemHistory,
                    new ItemHistoryEntity
                    {
                        ItemId = itemHistory.ItemId,
                        UserId = user.Id,
                        Action = action.ToString(),
                    }
                );
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
