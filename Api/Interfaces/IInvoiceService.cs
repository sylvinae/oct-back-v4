using API.Entities.Invoice;
using API.Models.Invoice;

namespace API.Interfaces;

public interface IInvoiceService
{
    Task<(FailedResponseInvoiceModel? failed, ResponseInvoiceModel? success)> CreateInvoice(
        CreateInvoiceModel Invoice
    );
    Task<bool> VoidInvoice(VoidInvoiceModel Invoice);
    IQueryable<InvoiceEntity> GetInvoices();
}
