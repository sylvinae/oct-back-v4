using API.Models.Item;

namespace Api.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(AdddItemHistoryModel itemHistory, ActionType action);

    Task<List<ResponseItemHistoryModel>> GetItemHistory(Guid userId);
}
