using API.Db;
using API.Entities.Item;
using API.Interfaces;
using API.Interfaces.Item;

namespace API.Services.Item;

public class GetItemService(Context _db, ILogger<GetItemService> _log) : IGetItemService
{
    public IQueryable<ItemEntity> GetAllItems()
    {
        _log.LogInformation("Getting all items.");
        return _db.Items;
    }
}
