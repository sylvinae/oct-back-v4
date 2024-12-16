using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Item;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Invoice;

public class InvoiceItemEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid InvoiceId { get; set; }

    [ForeignKey("InvoiceId")] public InvoiceEntity Invoice { get; set; } = null!;

    [Required] public Guid ItemId { get; set; }

    [ForeignKey("ItemId")] public ItemEntity Item { get; set; } = null!;

    // Interfaces details at purchase
    public int? ItemsSold { get; set; }
    public int? UsesConsumed { get; set; }

    [Required] [Precision(18, 2)] public decimal ItemPrice { get; set; }

    public decimal? DiscountedPrice { get; set; }
}