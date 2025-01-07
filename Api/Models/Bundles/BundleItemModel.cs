namespace API.Models.Bundles;

public class BundleItemModel
{
    public Guid BundleId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public int Uses { get; set; }
}