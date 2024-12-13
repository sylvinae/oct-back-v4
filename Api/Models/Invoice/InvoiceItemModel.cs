namespace API.Models.Invoice;

public class InvoiceItemModel
{
    public Guid? InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public int? ItemsSold { get; set; }
    public int? UsesConsumed { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
}