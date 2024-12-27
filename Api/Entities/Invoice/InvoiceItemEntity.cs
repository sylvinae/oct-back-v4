using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Products;
using Microsoft.EntityFrameworkCore;

// Adjusted for ProductEntity instead of ItemEntity

namespace API.Entities.Invoice;

public class InvoiceItemEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public Guid InvoiceId { get; set; }
    [ForeignKey("InvoiceId")] public InvoiceEntity Invoice { get; set; } = null!;
    [Required] public Guid ProductId { get; set; }
    [ForeignKey("ProductId")] public ProductEntity Product { get; set; } = null!;
    public int? QuantitySold { get; set; }
    public int? UsesConsumed { get; set; }
    [Required] [Precision(18, 2)] public decimal PurchasePrice { get; set; }
    [Precision(18, 2)] public decimal? DiscountedPrice { get; set; }
}