using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IUpdateItemService
{
    Task<(List<ResponseItemModel> ok, List<BulkFailure<UpdateItemModel>> fails)> UpdateItems(
        List<UpdateItemModel> items);
}