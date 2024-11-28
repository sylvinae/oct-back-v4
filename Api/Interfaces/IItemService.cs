using API.Models.Item;
using Data.Entities.Item;

namespace API.Interfaces;

public interface IItemService
{
    //multiple items
    Task<(List<ResponseItemModel> failed, List<ResponseItemModel> created)> CreateItems(
        List<CreateItemModel> items
    );
    Task<(List<ResponseItemModel> items, int totalCount)> GetItems(
        int page,
        int limit,
        bool includeHistory
    );
    Task<(List<ResponseItemModel> failed, List<ResponseItemModel> updated)> UpdateItems(
        List<UpdateItemModel> items
    );
    Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds);
    Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds);
    Task<(List<ResponseItemModel> items, int count)> SearchItems(
        string query,
        int page,
        int limit,
        bool isdeleted,
        bool isExpired,
        bool isReagent,
        bool isLow,
        bool includeHistory,
        bool? hasExpiry
    );

    //single items
    Task<ResponseItemModel?> CreateItem(CreateItemModel item);
    Task<ResponseItemModel?> GetItem(Guid id, bool includeHistory);
    Task<ResponseItemModel?> UpdateItem(UpdateItemModel item);
    Task<Guid?> DeleteItem(Guid itemId);
    Task<Guid?> RestoreItem(Guid itemId);
}
