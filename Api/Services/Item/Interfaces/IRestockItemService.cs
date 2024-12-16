using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IRestockItemService
{
    Task<bool> RestockItemsAsync(
        List<CreateRestockItemModel> restockItems);
}