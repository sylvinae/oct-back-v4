using API.Entities.Invoice;
using API.Models.Invoice;

namespace API.Interfaces;

public interface IInvoiceService
{
    Task<(FailedResponseInvoiceModel? failed, ResponseInvoiceModel? success)> CreateInvoice(
        CreateInvoiceModel invoice
    );

    Task<bool> VoidInvoice(VoidInvoiceModel invoice);
    IQueryable<InvoiceEntity> GetInvoices();
}