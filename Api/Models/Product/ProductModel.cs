namespace API.Models.Product;

public class ProductModel
{
    public string? Barcode { get; set; }
    public decimal RetailPrice { get; set; }
    public int Stock { get; set; }
}