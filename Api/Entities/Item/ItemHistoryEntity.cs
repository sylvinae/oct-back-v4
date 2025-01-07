using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Item;

public class ItemHistoryEntity : ProductHistoryEntity
{
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Classification { get; set; }
    public string? Formulation { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }
    [Precision(18, 2)] public decimal WholesalePrice { get; set; }
    [Required] public int LowThreshold { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Expiry { get; set; }

    public bool IsReagent { get; set; }

    [Required] public string Hash { get; set; } = null!;

    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }
}