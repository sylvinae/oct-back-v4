namespace API.Models.Item;

public class BaseItemHistoryModel
{
    public Guid ItemId { get; set; }
    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public int Stock { get; set; }
    public int LowThreshold { get; set; }
    public bool IsLow { get; set; }
    public bool IsReagent { get; set; }
    public int? UsesLeft { get; set; }
    public int? UsesMax { get; set; }
    public bool HasExpiry { get; set; }
    public DateTime? Expiry { get; set; }

    public bool IsExpired { get; set; }
    public bool IsDeleted { get; set; }
    public string Hash { get; set; } = null!;
    public string? Action { get; set; }
}

public class AddItemHistoryModel : BaseItemHistoryModel;

public class ResponseItemHistoryModel : BaseItemHistoryModel
{
    public Guid Id { get; set; }
    public DateTime ActionTaken { get; set; }
}