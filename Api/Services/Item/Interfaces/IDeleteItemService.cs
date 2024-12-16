namespace API.Services.Item.Interfaces;

public interface IDeleteItemService
{
    Task<bool> DeleteItems(List<Guid> itemIds);
}