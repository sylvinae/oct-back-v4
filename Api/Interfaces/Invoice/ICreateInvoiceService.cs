using API.Models.Invoice;

namespace API.Interfaces.Invoice;

public interface ICreateInvoiceService
{
    Task<(FailedResponseInvoiceModel? failed, ResponseInvoiceModel? success)> CreateInvoice(
        CreateInvoiceModel invoice
    );
}