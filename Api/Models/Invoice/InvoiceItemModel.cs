namespace API.Models.Invoice;

public class InvoiceItemModel
{
    public Guid? InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantitySold { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
}