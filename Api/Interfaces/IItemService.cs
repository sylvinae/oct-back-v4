using API.Entities.Item;
using API.Models.Item;

namespace API.Interfaces;

public interface IItemService
{
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> created)> CreateItems(
        List<CreateItemModel> items
    );
    Task<(List<ResponseItemModel> items, int totalCount)> GetItems(
        int page,
        int limit,
        bool includeHistory,
        bool isDeleted,
        bool isExpired
    );
    Task<(List<FailedResponseItemModel> failed, List<ResponseItemModel> updated)> UpdateItems(
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

    Task<ResponseItemModel?> GetItem(Guid id, bool includeHistory);
}
