using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface ICreateItemService
{
    Task<(List<ResponseItemModel> ok, List<BulkFailure<CreateItemModel>> fails)> CreateItems(
        List<CreateItemModel> items);
}