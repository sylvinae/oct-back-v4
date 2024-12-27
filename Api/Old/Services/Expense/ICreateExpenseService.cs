using API.Models;
using API.Models.Expense;

namespace API.Services.Expense;

public interface ICreateExpenseService
{
    Task<BulkFailure<ExpenseModel>?> CreateExpense(ExpenseModel expense);
}