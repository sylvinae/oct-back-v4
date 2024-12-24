using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IUpdateItemService
{
    Task<List<BulkFailure<UpdateItemModel>>?> UpdateItems(
        List<UpdateItemModel> items);
}