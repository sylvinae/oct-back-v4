using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface ICreateItemService
{
    Task<List<BulkFailure<CreateItemModel>>?> CreateItems(
        List<CreateItemModel> items);
}