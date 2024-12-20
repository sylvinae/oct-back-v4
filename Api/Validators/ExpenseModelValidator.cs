using API.Models.Expense;
using FluentValidation;

namespace API.Validators;

public class ExpenseItemModelValidator : AbstractValidator<ExpenseItemModel>
{
    public ExpenseItemModelValidator()
    {
        RuleFor(x => x.Details)
            .NotEmpty()
            .WithMessage("Details are required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.");
    }
}

public class ExpenseModelValidator : AbstractValidator<ExpenseModel>
{
    public ExpenseModelValidator()
    {
        RuleFor(x => x.ExpenseDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("ExpenseDate cannot be in the future.");

        RuleFor(x => x.TotalCost)
            .GreaterThan(0)
            .WithMessage("TotalCost must be greater than 0.");

        RuleFor(x => x.ExpenseItems)
            .NotEmpty()
            .WithMessage("ExpenseItems must have at least one item.")
            .ForEach(item => { item.SetValidator(new ExpenseItemModelValidator()); });
    }
}