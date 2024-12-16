using API.Models.Invoice;

namespace API.Services.Invoice.Interfaces;

public interface ICreateInvoiceService
{
    Task<(FailedResponseInvoiceModel? failed, ResponseInvoiceModel? success)> CreateInvoice(
        CreateInvoiceModel invoice
    );
}