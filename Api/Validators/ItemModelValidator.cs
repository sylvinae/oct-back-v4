using API.Models.Item;
using FluentValidation;

namespace API.Validators;

public class BaseItemModelValidator<T> : AbstractValidator<T>
    where T : BaseItemModel
{
    public BaseItemModelValidator()
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
            .GreaterThan(0)
            .When(x => x.UsesMax.HasValue)
            .WithMessage("UsesMax must be greater than 0 if specified.");

        RuleFor(x => x.UsesMax)
            .Null()
            .When(x => x.IsReagent == false)
            .WithMessage("UsesMax must be null when isReagent is false.");

        RuleFor(x => x.UsesLeft)
            .Null()
            .When(x => x.IsReagent == false)
            .WithMessage("UsesLeft must be null when isReagent is false.");

        RuleFor(x => x.Expiry)
            .Must((model, expiry) => expiry.HasValue || !model.HasExpiry)
            .When(x => x.HasExpiry)
            .WithMessage("Expiry is required when hasExpiry is true.");

        RuleFor(x => x.Expiry)
            .Must(BeTodayOrFutureDate)
            .When(x => x.HasExpiry && x.Expiry.HasValue)
            .WithMessage("Expiry must not be a past date.");
    }

    private bool BeTodayOrFutureDate(DateTime? expiry)
    {
        if (expiry.HasValue)
        {
            var expiryDate = expiry.Value.Date;
            return expiryDate >= DateTime.UtcNow.Date;
        }
        return false;
    }
}

public class CreateItemModelValidator : BaseItemModelValidator<CreateItemModel>
{
    public CreateItemModelValidator() { }
}

public class UpdateItemModelValidator : BaseItemModelValidator<UpdateItemModel>
{
    public UpdateItemModelValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required.");
        RuleFor(x => x.Hash).NotEmpty().WithMessage("Hash is required.");
    }
}
