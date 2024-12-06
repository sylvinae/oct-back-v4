using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace API.Models.Invoice;

public class BaseInvoiceModel
{
    [JsonProperty("userId")]
    public Guid? UserId { get; set; }

    [JsonProperty("invoiceDate")]
    [Column(TypeName = "timestamp without time zone")]
    public DateTime InvoiceDate { get; set; }

    [JsonProperty("amountTendered")]
    public decimal AmountTendered { get; set; }

    [JsonProperty("totalPrice")]
    public decimal TotalPrice { get; set; }

    [JsonProperty("totalDiscountedPrice")]
    public decimal? TotalDiscountedPrice { get; set; }

    [JsonProperty("isVoided")]
    public bool? IsVoided { get; set; }

    [JsonProperty("voidReason")]
    public string? VoidReason { get; set; }
}

public class ResponseInvoiceModel : BaseInvoiceModel
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("invoiceItems")]
    public ICollection<InvoiceItemModel>? InvoiceItems { get; set; } = [];
}

public class CreateInvoiceModel : BaseInvoiceModel
{
    [JsonProperty("invoiceItems")]
    public ICollection<InvoiceItemModel> InvoiceItems { get; set; } = [];
}

public class VoidInvoiceModel
{
    [JsonProperty("invoiceId")]
    [Required]
    public Guid InvoiceId { get; set; }

    [JsonProperty("voidReason")]
    [Required]
    [MaxLength(500)]
    public string VoidReason { get; set; } = string.Empty;
}
