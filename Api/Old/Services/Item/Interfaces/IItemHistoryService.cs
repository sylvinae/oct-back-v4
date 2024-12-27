using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IItemHistoryService
{
    Task<bool> AddItemHistory(AddItemHistoryModel itemHistory);
    Task<bool> AddItemHistoryRange(List<AddItemHistoryModel> itemHistory);
}