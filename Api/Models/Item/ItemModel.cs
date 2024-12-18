namespace API.Models.Item;

public class BaseItemModel
{
    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }
    public decimal Wholesale { get; set; }
    public decimal Retail { get; set; }
    public int Stock { get; set; }
    public int LowThreshold { get; set; }
    public bool IsReagent { get; set; }
    public int? UsesLeft { get; set; }
    public int? UsesMax { get; set; }
    public bool HasExpiry { get; set; }
    public DateTime? Expiry { get; set; }
}

public class CreateItemModel : BaseItemModel;

public class UpdateItemModel : BaseItemModel
{
    public Guid Id { get; set; }
}

public class CreateRestockItemModel : BaseItemModel;

public class ResponseItemModel : BaseItemModel
{
    public Guid Id { get; set; }
    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<ResponseItemHistoryModel>? ItemHistory { get; set; } = [];
}