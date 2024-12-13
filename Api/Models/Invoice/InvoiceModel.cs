using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Invoice;

public class BaseInvoiceModel
{
    public Guid? UserId { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime InvoiceDate { get; set; }

    public decimal AmountTendered { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? TotalDiscountedPrice { get; set; }
    public bool? IsVoided { get; set; }
    public string? VoidReason { get; set; }
}

public class ResponseInvoiceModel : BaseInvoiceModel
{
    public Guid Id { get; set; }
    public ICollection<InvoiceItemModel>? InvoiceItems { get; set; } = [];
}

public class FailedResponseInvoiceModel : BaseInvoiceModel
{
    public string? Error { get; set; }
}

public class CreateInvoiceModel : BaseInvoiceModel
{
    public ICollection<InvoiceItemModel> InvoiceItems { get; set; } = [];
}

public class VoidInvoiceModel
{
    [Required] public Guid Id { get; set; }
    [Required] [MaxLength(500)] public string VoidReason { get; set; } = string.Empty;
}