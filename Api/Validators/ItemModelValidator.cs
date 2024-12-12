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
        RuleFor(x => x.Wholesale)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Wholesale price must be non-negative.");
        RuleFor(x => x.Retail)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retail price must be non-negative.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock must be non-negative.");
        RuleFor(x => x.LowThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LowThreshold must be non-negative.");
        RuleFor(x => x.UsesMax)
            .Must((x, usesMax) => x.IsReagent ? usesMax > 0 : usesMax == null)
            .WithMessage("UsesMax must be greater than 0 when IsReagent is true, otherwise it must be null.");
        RuleFor(x => x.UsesLeft)
            .Must((x, usesLeft) => x.IsReagent ? usesLeft > 0 && usesLeft <= x.UsesMax : usesLeft == null)
            .WithMessage(
                "UsesLeft must be greater than 0 and not greater than UsesMax when IsReagent is true, otherwise it must be null.");
        RuleFor(x => x.Expiry)
            .Must((model, expiry) => expiry.HasValue || !model.HasExpiry)
            .When(x => x.HasExpiry)
            .WithMessage("Expiry is required when hasExpiry is true.");

        RuleFor(x => x.Expiry)
            .Must(BeTodayOrFutureDate)
            .When(x => x.HasExpiry && x.Expiry.HasValue)
            .WithMessage("Expiry must not be a past date.");
    }

    private static bool BeTodayOrFutureDate(DateTime? expiry)
    {
        if (!expiry.HasValue) return false;
        var expiryDate = expiry.Value.Date;
        return expiryDate >= DateTime.UtcNow.Date;
    }
}

public class CreateItemModelValidator : BaseItemModelValidator<CreateItemModel>;

public class UpdateItemModelValidator : BaseItemModelValidator<UpdateItemModel>
{
    public UpdateItemModelValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required.");
    }
}