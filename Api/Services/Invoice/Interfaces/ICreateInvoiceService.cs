using API.Models;
using API.Models.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface ICreateInvoiceService
{
    Task<BulkFailure<CreateInvoiceModel>> CreateInvoice(CreateInvoiceModel invoice);
}