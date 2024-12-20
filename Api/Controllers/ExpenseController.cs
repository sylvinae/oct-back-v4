using API.Models.Expense;
using API.Services.Expense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/expenses")]
[ApiController]
public class ExpenseController(ICreateExpenseService c) : ControllerBase
{
    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseModel expense)
    {
        var (ok, fail) = await c.CreateExpense(expense);
        if (fail != null) return BadRequest(new { status = "fail", fail.Errors });
        return Ok(new { status = "success", expense = ok });
    }
}