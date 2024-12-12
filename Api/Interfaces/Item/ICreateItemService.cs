using API.Models.Item;

namespace API.Interfaces.Item;

public interface ICreateItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> created)> CreateItems(
        List<CreateItemModel> items
    );
}