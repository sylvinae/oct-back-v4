using API.Models.Item;

namespace API.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(AddItemHistoryModel itemHistory, ActionType action);

    Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid userId);
}
