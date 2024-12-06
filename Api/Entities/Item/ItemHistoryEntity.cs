using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Item;
using API.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace API.Item.ItemHistory;

public class ItemHistoryEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [ForeignKey("ItemId")]
    public ItemEntity Item { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public UserEntity User { get; set; } = null!;

    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }

    [Precision(18, 2)]
    public decimal Wholesale { get; set; }

    [Precision(18, 2)]
    public decimal Retail { get; set; }

    [Required]
    public int Stock { get; set; }

    [Required]
    public int LowThreshold { get; set; }
    public string? Company { get; set; }
    public bool HasExpiry { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Expiry { get; set; }
    public bool IsReagent { get; set; }
    public int? UsesLeft { get; set; }
    public int? UsesMax { get; set; }
    public bool IsDeleted { get; set; }

    [Required]
    public string Hash { get; set; } = null!;
    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }
    public string Action { get; set; } = null!;
}
