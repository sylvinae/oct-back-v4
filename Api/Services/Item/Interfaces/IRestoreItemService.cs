using API.Models;

namespace API.Services.Item.Interfaces;

public interface IRestoreItemService
{
    Task<(List<Guid> ok, List<BulkFailure<Guid>> fails)> RestoreItems(List<Guid> itemIds);
}