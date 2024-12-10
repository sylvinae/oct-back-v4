using API.Entities.Invoice;

namespace API.Interfaces.Invoice;

public interface IGetInvoiceService
{
    IQueryable<InvoiceEntity> GetInvoices();
}