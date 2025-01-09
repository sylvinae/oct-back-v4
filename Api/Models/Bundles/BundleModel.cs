namespace API.Models.Bundles;

public class BaseBundleModel
{
    public string BundleName { get; set; } = null!;
    public decimal RetailPrice { get; set; }
    public int Stock { get; set; }
    public string? Barcode { get; set; }
    public List<BundleItemModel> BundleItems { get; set; } = [];
}

public class CreateBundleModel : BaseBundleModel;

public class UpdateBundleModel : BaseBundleModel
{
    public Guid Id { get; set; }
}

public class DeleteBundleModel
{
    public Guid Id { get; set; }
}