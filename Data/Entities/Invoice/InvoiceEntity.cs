using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.User;

namespace Data.Entities.Invoice;

public class InvoiceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string InvoiceDate { get; set; } = null!;

    [Required]
    public decimal AmountTendered { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }
    public decimal? TotalDiscountedPrice { get; set; }
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }

    [ForeignKey("UserId")]
    public UserEntity User { get; set; } = null!;

    public ICollection<InvoiceItemEntity>? InvoiceItems = [];
}
