namespace API.Interfaces.Item;

public interface IDeleteItemService
{
    Task<(List<Guid> failed, List<Guid> deleted)> DeleteItems(List<Guid> itemIds);
}
