using API.Models.Item;

namespace API.Interfaces.Item;

public interface IRestockItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> restocked, List<ResponseItemModel> created)>
        RestockItemsAsync(
            List<CreateRestockItemModel> restockItems);
}