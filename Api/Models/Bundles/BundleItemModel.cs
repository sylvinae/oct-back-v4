namespace API.Models.Bundles;

public class BundleItemModel
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public int Uses { get; set; }
}

public class UpdateBundleItemModel : BundleItemModel
{
    public Guid? Id { get; set; }
}