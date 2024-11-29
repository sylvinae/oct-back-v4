using API.Entities.Item;
using API.Models.Item;

namespace API.Interfaces;

public interface IItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> created)> CreateItems(
        List<CreateItemModel> items
    );

    IQueryable<ItemEntity> GetAllItems();
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> updated)> UpdateItems(
        List<UpdateItemModel> items
    );
    Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds);
    Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds);
}
