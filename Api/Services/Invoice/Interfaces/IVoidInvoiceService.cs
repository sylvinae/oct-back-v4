using API.Models.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface IVoidInvoiceService
{
    Task<bool> VoidInvoice(VoidInvoiceModel invoice);
}