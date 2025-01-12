using API.Models;
using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IUpdateItemService
{
    Task<BulkFailure<UpdateItemModel>?> UpdateItem(
        UpdateItemModel items);
}