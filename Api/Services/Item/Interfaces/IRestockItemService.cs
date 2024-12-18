using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IRestockItemService
{
    Task<(List<ResponseItemModel> ok, List<BulkFailure<CreateRestockItemModel>> fails)> RestockItemsAsync(
        List<CreateRestockItemModel> restockItems);
}