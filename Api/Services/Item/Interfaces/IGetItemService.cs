using API.Entities.Item;

namespace API.Services.Item.Interfaces;

public interface IGetItemService
{
    IQueryable<ItemEntity> GetAllItems();
}