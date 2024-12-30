using API.Db;
using API.Entities.Item;
using API.Services.Item.Interfaces;

namespace API.Services.Item;

public class GetItemService(Context db, ILogger<GetItemService> log) : IGetItemService
{
    public IQueryable<ItemEntity> GetAllItems()
    {
        log.LogInformation("Getting all items.");
        return db.Products.OfType<ItemEntity>();
    }
}