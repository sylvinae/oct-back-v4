using API.Models;

namespace API.Services.Item.Interfaces;

public interface IRestoreItemService
{
    Task<List<BulkFailure<Guid>>?> RestoreItems(List<Guid> itemIds);
}