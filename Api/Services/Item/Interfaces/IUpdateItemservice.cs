using API.Models.Item;

namespace API.Services.Item.Interfaces;

public interface IUpdateItemService
{
    Task<bool> UpdateItems(List<UpdateItemModel> items);
}