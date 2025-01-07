using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(CreateItemHistoryModel itemHistory);
    Task<bool> AddItemHistoryRange(List<CreateItemHistoryModel> itemHistory);
}