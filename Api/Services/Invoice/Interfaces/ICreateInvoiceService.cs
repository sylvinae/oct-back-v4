using API.Models.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface ICreateInvoiceService
{
    Task<bool> CreateInvoice(CreateInvoiceModel invoice);
}