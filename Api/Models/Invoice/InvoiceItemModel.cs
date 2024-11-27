using Newtonsoft.Json;

namespace API.Models.Invoice;

public class BaseInvoiceItemModel
{
    [JsonProperty("invoiceId")]
    public Guid InvoiceId { get; set; }

    [JsonProperty("itemId")]
    public Guid ItemId { get; set; }

    // Item details at purchase
    [JsonProperty("itemName")]
    public required string ItemName { get; set; }

    [JsonProperty("itemQuantity")]
    public int? ItemQuantity { get; set; }

    [JsonProperty("usesConsumed")]
    public int? UsesConsumed { get; set; }

    [JsonProperty("itemPrice")]
    public decimal ItemPrice { get; set; }

    [JsonProperty("discountedPrice")]
    public decimal? DiscountedPrice { get; set; }
}

public class ResponseInvoiceItemModel : BaseInvoiceItemModel
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
}
