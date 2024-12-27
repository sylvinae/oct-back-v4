using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IRestockItemService
{
    Task<List<BulkFailure<CreateRestockItemModel>>?> RestockItemsAsync(
        List<CreateRestockItemModel> restockItems);
}