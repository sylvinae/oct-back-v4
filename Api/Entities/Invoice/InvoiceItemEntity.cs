using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Item;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Invoice;

public class InvoiceItemEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid InvoiceId { get; set; }

    [ForeignKey("InvoiceId")]
    public InvoiceEntity Invoice { get; set; } = null!;

    [Required]
    public Guid ItemId { get; set; }

    [ForeignKey("ItemId")]
    public ItemEntity Item { get; set; } = null!;

    // Item details at purchase
    public required string ItemName { get; set; }
    public int? ItemQuantity { get; set; }
    public int? UsesConsumed { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal ItemPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
}
