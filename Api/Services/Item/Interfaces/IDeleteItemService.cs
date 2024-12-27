using API.Models;

namespace API.Services.Item.Interfaces;

public interface IDeleteItemService
{
    Task<List<BulkFailure<Guid>>?> DeleteItems(List<Guid> itemIds);
}