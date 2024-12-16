using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(AddItemHistoryModel itemHistory, ActionType action);
    Task<bool> AddItemHistoryRange(List<AddItemHistoryModel> itemHistory, ActionType action);

    Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid userId);
}