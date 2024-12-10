using API.Entities.Item;

namespace API.Interfaces.Item;

public interface IGetItemService
{
    IQueryable<ItemEntity> GetAllItems();
}
