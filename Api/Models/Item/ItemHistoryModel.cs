namespace API.Models.Item;

public class CreateItemHistoryModel : BaseItemModel
{
    public Guid ItemId { get; set; }
    public string? Action { get; set; }
    public string Hash { get; set; } = null!;
}

public class ResponseItemHistoryModel : BaseItemModel
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string? Action { get; set; }
    public DateTime ActionTaken { get; set; }
    public string Hash { get; set; } = null!;
}