using API.Models.Item;
using FluentValidation;

namespace API.Validators;

public class BaseItemModelValidator<T> : AbstractValidator<T>
    where T : BaseItemModel
{
    protected BaseItemModelValidator()
    {
        RuleFor(x => x.Barcode).NotEmpty().WithMessage("Barcode is required.");
        RuleFor(x => x.Brand).NotEmpty().WithMessage("Brand is required.");
        RuleFor(x => x.WholesalePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Wholesale price must be non-negative.");
        RuleFor(x => x.RetailPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retail price must be non-negative.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock must be non-negative.");
        RuleFor(x => x.LowThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LowThreshold must be non-negative.");
        RuleFor(x => x.Expiry)
            .Must(BeTodayOrFutureDate)
            .When(x => x.Expiry.HasValue)
            .WithMessage("Expiry must not be a past date.");

        RuleFor(x => x.UsesMax).NotNull().When(x => x.IsReagent)
            .WithMessage("UsesMax is required since this item is a reagent.");
        RuleFor(x => x.UsesLeft).NotNull().When(x => x.IsReagent)
            .WithMessage("UsesLeft is required since this item is a reagent.");
    }

    private static bool BeTodayOrFutureDate(DateTime? expiry)
    {
        if (!expiry.HasValue) return false;
        var expiryDate = expiry.Value.Date;
        return expiryDate >= DateTime.Now.Date;
    }
}

public class UpdateItemModelValidator : BaseItemModelValidator<UpdateItemModel>
{
    public UpdateItemModelValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required.");
    }
}

public class CreateItemModelValidator : BaseItemModelValidator<CreateItemModel>;

public class CreateRestockItemModelValidator : BaseItemModelValidator<CreateRestockItemModel>;