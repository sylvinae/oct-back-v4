using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface ICreateItemService
{
    Task<bool> CreateItems(
        List<CreateItemModel> items
    );
}