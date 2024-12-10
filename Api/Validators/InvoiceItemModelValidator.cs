using API.Models.Invoice;
using FluentValidation;

namespace API.Validators;

public class InvoiceItemModelValidator : AbstractValidator<InvoiceItemModel>
{
    public InvoiceItemModelValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("ItemId is required.");

        RuleFor(x => x.ItemsSold)
            .GreaterThan(0)
            .When(x => x.ItemsSold.HasValue)
            .WithMessage("ItemQuantity must be greater than 0 if specified.");

        RuleFor(x => x.UsesConsumed)
            .GreaterThanOrEqualTo(0)
            .When(x => x.UsesConsumed.HasValue)
            .WithMessage("UsesConsumed must be non-negative if specified.");

        RuleFor(x => x.ItemPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ItemPrice must be non-negative.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DiscountedPrice.HasValue)
            .WithMessage("DiscountedPrice must be non-negative if specified.");
    }
}
