using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Invoice;
using API.Item.ItemHistory;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Entities.Item;

public class ItemEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

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

    public int LowThreshold { get; set; }

    public string? Company { get; set; }

    public bool HasExpiry { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Expiry { get; set; }

    public bool IsReagent { get; set; }

    public int? UsesLeft { get; set; }

    public int? UsesMax { get; set; }

    public string Hash { get; set; } = null!;

    public bool IsLow { get; set; }

    public bool IsExpired { get; set; }
    public bool IsDeleted { get; set; }

    [JsonIgnore]
    public ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = [];
    public ICollection<ItemHistoryEntity> ItemHistory { get; set; } = [];
}
