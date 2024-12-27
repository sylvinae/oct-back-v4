using System.ComponentModel.DataAnnotations;
using API.Entities.Invoice;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Products;

public class ProductEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Name { get; set; } = null!;
    [Required] [Precision(18, 2)] public decimal RetailPrice { get; set; }
    [Required] public int Stock { get; set; }

    public string? Barcode { get; set; }

    [Required] public string ProductType { get; set; } = null!;

    public ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = (List<InvoiceItemEntity>) [];
}