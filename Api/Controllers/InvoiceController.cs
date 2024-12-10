using API.Entities.Invoice;
using API.Interfaces;
using API.Models.Invoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace API.Controllers;

[Route("api/invoices")]
[ApiController]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<InvoiceEntity>> GetAllInvoices()
    {
        return Ok(invoiceService.GetInvoices());
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceModel InvoiceModel)
    {
        var (failed, created) = await invoiceService.CreateInvoice(InvoiceModel);
        if (created == null)
            return BadRequest(new { failed });

        return CreatedAtAction(nameof(CreateInvoice), new { id = created.Id }, new { created });
    }

    [HttpPost("void")]
    public async Task<IActionResult> VoidInvoice([FromBody] VoidInvoiceModel voidModel)
    {
        var result = await invoiceService.VoidInvoice(voidModel);
        return result ? Ok() : BadRequest();
    }
}
