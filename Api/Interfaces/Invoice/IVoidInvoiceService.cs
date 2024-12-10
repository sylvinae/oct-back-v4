using API.Models.Invoice;

namespace API.Interfaces.Invoice;

public interface IVoidInvoiceService
{
    Task<bool> VoidInvoice(VoidInvoiceModel invoice);
}