using API.Models.Expense;
using API.Services.Expense.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/expenses")]
[ApiController]
public class ExpenseController(ICreateExpenseService c) : ControllerBase
{
    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseModel expense)
    {
        var fail = await c.CreateExpense(expense);
        if (fail != null) return BadRequest(fail.Errors);
        return Ok();
    }
}