using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Products;

public class ProductHistoryEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid ProductId { get; set; }
    [ForeignKey("ProductId")] public ProductEntity Product { get; set; } = null!;

    [Required] public Guid UserId { get; set; }
    [ForeignKey("UserId")] public UserEntity User { get; set; } = null!;

    public string? Barcode { get; set; }
    [Required] [Precision(18, 2)] public decimal RetailPrice { get; set; }
    [Required] public int Stock { get; set; }
    [Required] public bool IsDeleted { get; set; }

    [Required] public string Action { get; set; } = null!;

    [Required]
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ActionTaken { get; set; } = DateTime.Now;
}