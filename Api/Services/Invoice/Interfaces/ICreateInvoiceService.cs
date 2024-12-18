using API.Models;
using API.Models.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface ICreateInvoiceService
{
    Task<(bool ok, BulkFailure<CreateInvoiceModel>? fail)> CreateInvoice(CreateInvoiceModel invoice);
}