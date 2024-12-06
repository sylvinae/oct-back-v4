using Newtonsoft.Json;

namespace API.Models.Invoice;

public class InvoiceItemModel
{
    [JsonProperty("invoiceId")]
    public Guid InvoiceId { get; set; }

    [JsonProperty("itemId")]
    public Guid ItemId { get; set; }

    [JsonProperty("itemQuantity")]
    public int? ItemQuantity { get; set; }

    [JsonProperty("usesConsumed")]
    public int? UsesConsumed { get; set; }

    [JsonProperty("itemPrice")]
    public decimal ItemPrice { get; set; }

    [JsonProperty("discountedPrice")]
    public decimal? DiscountedPrice { get; set; }
}
