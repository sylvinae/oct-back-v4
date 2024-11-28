using API.Models.Item;

namespace API.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(AdddItemHistoryModel itemHistory, ActionType action);

    Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid userId);
}
