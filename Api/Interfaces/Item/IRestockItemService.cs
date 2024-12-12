using API.Models.Item;

namespace API.Interfaces.Item;

public interface IRestockItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> success)> RestockItemsAsync(
        List<CreateRestockItemModel> restockItems);
}