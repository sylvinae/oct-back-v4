using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Bundles;

public class BundleHistoryEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public Guid BundleId { get; set; } = Guid.NewGuid();
    [ForeignKey("BundleId")] public BundleEntity Bundle { get; set; } = null!;

    [Required] public string Name { get; set; } = null!;
    [Required] [Precision(18, 2)] public decimal Price { get; set; }
    [Required] public int Stock { get; set; }
    public string? Barcode { get; set; }

    [Required] public bool IsDeleted { get; set; }
}