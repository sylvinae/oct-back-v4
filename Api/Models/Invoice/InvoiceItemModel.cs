namespace API.Models.Invoice;

public class InvoiceItemModel
{
    public Guid? InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public int ItemsSold { get; set; }
    public decimal? DiscountedPrice { get; set; }
}