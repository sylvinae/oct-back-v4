using API.Models.Item;
using Data.Entities.Item;

namespace API.Interfaces;

public interface IItemService
{
    Task<(List<ResponseItemModel> failed, List<ResponseItemModel> created)> CreateItems(
        List<CreateItemModel> items
    );
    Task<(List<ResponseItemModel> items, int totalCount)> GetItems(
        int page,
        int limit,
        bool includeHistory
    );
    Task<ResponseItemModel?> GetItem(Guid id, bool includeHistory);
    Task<(List<ResponseItemModel> failed, List<ResponseItemModel> updated)> UpdateItems(
        List<UpdateItemModel> items
    );
    Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds);
    Task<(List<Guid> failed, List<Guid> undeleted)> UndeleteItems(List<Guid> itemIds);
    Task<(List<ResponseItemModel> items, int count)> SearchItems(
        string query,
        int page,
        int limit,
        bool includeHistory = false
    );
}
