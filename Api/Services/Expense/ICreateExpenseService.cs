using API.Models;
using API.Models.Expense;

namespace API.Services.Expense;

public interface ICreateExpenseService
{
    Task<(ResponseExpenseModel? ok, BulkFailure<ExpenseModel>? fail)> CreateExpense(ExpenseModel expense);
}