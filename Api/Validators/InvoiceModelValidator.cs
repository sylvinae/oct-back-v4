using System;
using API.Models.Invoice;
using FluentValidation;

namespace API.Validators
{
    // Validator for BaseInvoiceModel, validating shared properties
    public class BaseInvoiceModelValidator : AbstractValidator<BaseInvoiceModel>
    {
        public BaseInvoiceModelValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");

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

    // Validator for CreateInvoiceModel, which includes InvoiceItems
    public class CreateInvoiceModelValidator : AbstractValidator<CreateInvoiceModel>
    {
        public CreateInvoiceModelValidator()
        {
            // Base validation for the shared properties from BaseInvoiceModel
            Include(new BaseInvoiceModelValidator());

            // Additional validation for InvoiceItems in CreateInvoiceModel
            RuleFor(x => x.InvoiceItems)
                .NotEmpty()
                .WithMessage("Invoice items are required.")
                .Must(items => items.Count > 0)
                .WithMessage("At least one invoice item is required.");

            // Further item-level validation can go here, if needed (e.g., each item in the collection)
        }
    }

    // Validator for VoidInvoiceModel
    public class VoidInvoiceModelValidator : AbstractValidator<VoidInvoiceModel>
    {
        public VoidInvoiceModelValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required.");

            RuleFor(x => x.VoidReason)
                .NotEmpty()
                .WithMessage("Void reason is required.")
                .MaximumLength(500)
                .WithMessage("Void reason cannot exceed 500 characters.");
        }
    }
}
