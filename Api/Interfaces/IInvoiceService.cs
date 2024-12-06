using API.Models.Invoice;

namespace API.Interfaces;

public interface IInvoiceService
{
    Task<ResponseInvoiceModel?> CreateSale(CreateInvoiceModel invoice);
    Task<bool> VoidSale(VoidInvoiceModel invoice);
    Task<List<ResponseInvoiceModel>?> GetSales();
}
