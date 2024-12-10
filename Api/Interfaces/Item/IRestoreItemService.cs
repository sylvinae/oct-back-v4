namespace API.Interfaces.Item;

public interface IRestoreItemService
{
    Task<(List<Guid> failed, List<Guid> restored)> RestoreItems(List<Guid> itemIds);
}
