using API.Db;
using API.Entities.Expense;
using API.Entities.User;
using API.Models;
using API.Models.Expense;
using API.Services.Item;
using API.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace API.Services.Expense;

public class CreateExpenseService(
    Context db,
    ILogger<CreateItemService> log,
    IValidator<ExpenseModel> createValidator,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor) : ICreateExpenseService
{
    public async Task<BulkFailure<ExpenseModel>?> CreateExpense(ExpenseModel expense)
    {
        log.LogInformation("Creating expenses...");

        var isValid = await createValidator.ValidateAsync(expense);

        if (!isValid.IsValid)
            return new BulkFailure<ExpenseModel>
            {
                Input = expense, Errors = isValid.Errors.ToDictionary(
                    e => e.PropertyName,
                    e => e.ErrorMessage)
            };

        var user = userManager.GetUserAsync(httpContextAccessor.HttpContext!.User).Result;
        var items = new List<ExpenseItemEntity>();
        var newExpense = PropCopier.Copy(expense, new ExpenseEntity { UserId = user!.Id });

        foreach (var item in expense.ExpenseItems)
            items.Add(PropCopier.Copy(item, new ExpenseItemEntity { ExpenseId = newExpense.Id }));

        newExpense.ExpenseItems = items;
        await db.Expenses.AddAsync(newExpense);
        await db.SaveChangesAsync();
        return null;
    }
}