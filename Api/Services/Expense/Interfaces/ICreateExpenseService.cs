using API.Models;
using API.Models.Expense;

namespace API.Services.Expense.Interfaces;

public interface ICreateExpenseService
{
    Task<BulkFailure<ExpenseModel>?> CreateExpense(CreateExpenseModel expense);
}