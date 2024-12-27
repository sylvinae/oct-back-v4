namespace API.Models.Item;

public class BaseItemModel
{
    public string Name { get; set; } = null!;
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public int Stock { get; set; }

    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }

    public int LowThreshold { get; set; }
    public DateTime? Expiry { get; set; }
}

public class CreateItemModel : BaseItemModel;

public class CreateRestockItemModel : BaseItemModel;

public class UpdateItemModel : BaseItemModel
{
    public Guid Id { get; set; }
}

public class ResponseItemModel : BaseItemModel
{
    public Guid Id { get; set; }
    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<ResponseItemHistoryModel>? ItemHistory { get; set; } = [];
}