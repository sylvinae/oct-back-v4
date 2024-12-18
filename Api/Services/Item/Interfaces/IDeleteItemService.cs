using API.Models;

namespace API.Services.Item.Interfaces;

public interface IDeleteItemService
{
    Task<(List<Guid> ok, List<BulkFailure<Guid>> fails)> DeleteItems(List<Guid> itemIds);
}