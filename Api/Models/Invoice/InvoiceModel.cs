using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Invoice;
using API.Entities.User;
using Newtonsoft.Json;

namespace API.Models.Invoice;

public class BaseInvoiceModel
{
    [JsonProperty("userId")]
    public Guid UserId { get; set; }

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
    public bool IsVoided { get; set; } = false;

    [JsonProperty("voidReason")]
    public string? VoidReason { get; set; }
}

public class ResponseInvoiceModel : BaseInvoiceModel
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("invoiceItems")]
    public ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = [];
}

public class CreateInvoiceModel : BaseInvoiceItemModel
{
    [JsonProperty("invoiceItems")]
    public ICollection<InvoiceItemEntity>? InvoiceItems { get; set; } = [];
}
