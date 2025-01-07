using API.Entities.Invoice;
using API.Models.Invoice;
using API.Services.Invoice.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers;

[Route("api/invoices")]
[ApiController]
public class InvoiceController(
    ICreateInvoiceService createInvoiceService,
    IVoidInvoiceService voidInvoiceService,
    IGetInvoiceService getInvoiceService
) : ControllerBase
{
    [Authorize(Roles = "admin,cashier")]
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<InvoiceEntity>> GetAllInvoices()
    {
        return Ok(getInvoiceService.GetInvoices());
    }

    [Authorize(Roles = "admin,cashier")]
    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceModel invoiceModel)
    {
        var result = await createInvoiceService.CreateInvoice(invoiceModel);

        return result.Errors.Count != 0 ? BadRequest(result.Errors) : Ok();
    }

    [Authorize(Roles = "admin,cashier")]
    [HttpPost("void")]
    public async Task<IActionResult> VoidInvoice([FromBody] VoidInvoiceModel voidModel)
    {
        var ok = await voidInvoiceService.VoidInvoice(voidModel);
        return ok ? Ok() : BadRequest();
    }
}