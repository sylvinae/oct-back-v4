using System.ComponentModel.DataAnnotations;
using API.Entities.Products;

namespace API.Entities.Bundles;

public class BundleEntity : ProductEntity
{
    [Required] public ICollection<BundleItemEntity> BundleItems { get; set; } = (List<BundleItemEntity>) [];
    [Required] public bool IsDeleted { get; set; }
}