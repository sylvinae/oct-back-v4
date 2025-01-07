using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Item;

namespace API.Entities.Bundles;

public class BundleItemEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid BundleId { get; set; }
    [ForeignKey("BundleId")] public BundleEntity Bundle { get; set; } = null!;

    [Required] public Guid ItemId { get; set; }
    [ForeignKey("ItemId")] public ItemEntity Item { get; set; } = null!;

    public int Quantity { get; set; }
    public int Uses { get; set; }
}