using API.Models.Invoice;
using FluentValidation;

namespace API.Validators;

public class BaseInvoiceModelValidator : AbstractValidator<BaseInvoiceModel>
{
    public BaseInvoiceModelValidator()
    {
        RuleFor(x => x.InvoiceDate)
            .NotEmpty()
            .WithMessage("Interfaces date is required.")
            .Must(invoiceDate => BeTodayOrFutureDate(invoiceDate))
            .WithMessage("Interfaces date must be in the future or today.");

        RuleFor(x => x.AmountTendered)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount tendered cannot be negative.")
            .GreaterThanOrEqualTo(x => x.TotalDiscountedPrice)
            .WithMessage("Amount tendered cannot be less than total discounted price.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total price cannot be negative.");

        RuleFor(x => x.TotalDiscountedPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalDiscountedPrice.HasValue)
            .WithMessage("Total discounted price cannot be negative.");
    }

    private static bool BeTodayOrFutureDate(DateTime? invoice)
    {
        if (!invoice.HasValue) return false;
        var invoiceDate = invoice.Value.Date;
        return invoiceDate >= DateTime.UtcNow.Date;
    }
}

public class CreateInvoiceModelValidator : AbstractValidator<CreateInvoiceModel>
{
    public CreateInvoiceModelValidator()
    {
        Include(new BaseInvoiceModelValidator());

        RuleFor(x => x.InvoiceItems)
            .NotEmpty()
            .WithMessage("Interfaces items are required.")
            .Must(items => items.Count > 0)
            .WithMessage("At least one Interfaces item is required.");

        RuleForEach(x => x.InvoiceItems).SetValidator(new InvoiceItemModelValidator());
    }
}

public class VoidInvoiceModelValidator : AbstractValidator<VoidInvoiceModel>
{
    public VoidInvoiceModelValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Interfaces ID is required.");

        RuleFor(x => x.VoidReason)
            .NotEmpty()
            .WithMessage("Void reason is required.")
            .MaximumLength(500)
            .WithMessage("Void reason cannot exceed 500 characters.");
    }
}