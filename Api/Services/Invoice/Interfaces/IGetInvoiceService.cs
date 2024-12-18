using API.Entities.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface IGetInvoiceService
{
    IQueryable<InvoiceEntity> GetInvoices();
}