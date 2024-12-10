using API.Entities.Invoice;
using API.Interfaces.Invoice;
using API.Models.Invoice;
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
        var (failed, created) = await createInvoiceService.CreateInvoice(invoiceModel);
        if (created == null)
            return BadRequest(new { failed });

        return CreatedAtAction(nameof(CreateInvoice), new { id = created.Id }, new { created });
    }

    [Authorize(Roles = "admin,cashier")]
    [HttpPost("void")]
    public async Task<IActionResult> VoidInvoice([FromBody] VoidInvoiceModel voidModel)
    {
        var result = await voidInvoiceService.VoidInvoice(voidModel);
        return result ? Ok() : BadRequest();
    }
}