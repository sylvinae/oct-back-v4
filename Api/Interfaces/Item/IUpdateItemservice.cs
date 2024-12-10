using API.Models.Item;

namespace API.Interfaces.Item;

public interface IUpdateItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> updated)> UpdateItems(
        List<UpdateItemModel> items
    );
}
