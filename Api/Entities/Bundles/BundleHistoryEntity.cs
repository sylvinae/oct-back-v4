using System.ComponentModel.DataAnnotations;
using API.Entities.Products;

namespace API.Entities.Bundles;

public class BundleHistoryEntity : ProductHistoryEntity

{
    [Required] public string BundleName { get; set; } = null!;
}