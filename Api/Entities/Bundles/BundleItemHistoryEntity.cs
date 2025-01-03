using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Item;
using API.Entities.User;

namespace API.Entities.Bundles;

public class BundleItemHistoryEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid BundleId { get; set; }
    [ForeignKey("BundleId")] public BundleEntity Bundle { get; set; } = null!;

    [Required] public Guid ItemId { get; set; }
    [ForeignKey("ItemId")] public ItemEntity Item { get; set; } = null!;

    [Required] public Guid UserId { get; set; }
    [ForeignKey("UserId")] public UserEntity User { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime ActionTaken { get; set; } = DateTime.Now;

    [MaxLength(50)] public string Action { get; set; } = null!;
    [Required] public int Quantity { get; set; }
}