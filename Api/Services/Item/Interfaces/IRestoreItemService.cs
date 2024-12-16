namespace API.Services.Item.Interfaces;

public interface IRestoreItemService
{
    Task<bool> RestoreItems(List<Guid> itemIds);
}