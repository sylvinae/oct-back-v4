namespace API.Models.Items;

public class BaseItemModel
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Company { get; set; }

    public int LowThreshold { get; set; }
    public DateTime? Expiry { get; set; }
}

public class CreateItemModel : BaseItemModel;