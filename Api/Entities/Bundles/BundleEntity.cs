using System.ComponentModel.DataAnnotations;
using API.Entities.Products;

namespace API.Entities.Bundles;

public class BundleEntity : ProductEntity
{
    [Required] public string BundleName { get; set; } = null!;
    [Required] public ICollection<BundleItemEntity> BundleItems { get; set; } = [];
}