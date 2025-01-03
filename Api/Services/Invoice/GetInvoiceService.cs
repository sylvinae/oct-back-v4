using API.Db;
using API.Entities.Invoice;
using API.Services.Invoice.Interfaces;

namespace API.Services.Invoice;

public class GetInvoiceService(
    ILogger<GetInvoiceService> log,
    Context db
) : IGetInvoiceService
{
    public IQueryable<InvoiceEntity> GetInvoices()
    {
        log.LogInformation("Get invoices called. OData support");
        return db.Invoices;
    }
}