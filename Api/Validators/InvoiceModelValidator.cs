using System;
using API.Models.Invoice;
using FluentValidation;

namespace API.Validators
{
    public class BaseInvoiceModelValidator : AbstractValidator<BaseInvoiceModel>
    {
        public BaseInvoiceModelValidator()
        {
            RuleFor(x => x.InvoiceDate).NotEmpty().WithMessage("Invoice date is required.");

            RuleFor(x => x.AmountTendered)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Amount tendered cannot be negative.");

            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total price cannot be negative.");

            RuleFor(x => x.TotalDiscountedPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.TotalDiscountedPrice.HasValue)
                .WithMessage("Total discounted price cannot be negative.");
        }
    }

    public class CreateInvoiceModelValidator : AbstractValidator<CreateInvoiceModel>
    {
        public CreateInvoiceModelValidator()
        {
            Include(new BaseInvoiceModelValidator());

            RuleFor(x => x.InvoiceItems)
                .NotEmpty()
                .WithMessage("Invoice items are required.")
                .Must(items => items.Count > 0)
                .WithMessage("At least one Invoice item is required.");
        }
    }

    public class VoidInvoiceModelValidator : AbstractValidator<VoidInvoiceModel>
    {
        public VoidInvoiceModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Invoice ID is required.");

            RuleFor(x => x.VoidReason)
                .NotEmpty()
                .WithMessage("Void reason is required.")
                .MaximumLength(500)
                .WithMessage("Void reason cannot exceed 500 characters.");
        }
    }
}
