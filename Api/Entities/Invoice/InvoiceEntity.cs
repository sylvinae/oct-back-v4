using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.User;

namespace API.Entities.Invoice;

public class InvoiceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Column(TypeName = "timestamp without time zone")]
    public DateTime InvoiceDate { get; set; }

    [Required]
    public decimal AmountTendered { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }
    public decimal? TotalDiscountedPrice { get; set; }
    public bool IsVoided { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? VoidTime { get; set; }
    public string? VoidReason { get; set; }

    [ForeignKey("UserId")]
    public UserEntity User { get; set; } = null!;

    public ICollection<InvoiceItemEntity>? InvoiceItems = [];
}
