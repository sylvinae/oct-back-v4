using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Item;

public class ItemEntity : ProductEntity
{
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }
    [Required] [Precision(18, 2)] public decimal WholesalePrice { get; set; }

    [Required] public string Hash { get; set; } = null!;

    public int LowThreshold { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Expiry { get; set; }

    public bool IsReagent { get; set; }
    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }

    public ICollection<ItemHistoryEntity> ItemHistory { get; set; } = new List<ItemHistoryEntity>();
}