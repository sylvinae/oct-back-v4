using API.Models.Invoice;
using FluentValidation;

namespace API.Validators;

public class InvoiceItemModelValidator : AbstractValidator<InvoiceItemModel>
{
    public InvoiceItemModelValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.QuantitySold)
            .GreaterThan(0)
            .When(x => x.QuantitySold <= 0)
            .WithMessage("ItemQuantity must be greater than 0 if specified.");


        RuleFor(x => x.DiscountedPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DiscountedPrice.HasValue)
            .WithMessage("DiscountedPrice must be non-negative if specified.");
    }
}