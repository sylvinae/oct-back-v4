using API.Models.Bundles;
using FluentValidation;

namespace API.Validators;

public class BaseBundleModelValidator<T> : AbstractValidator<T> where T : BaseBundleModel
{
    protected BaseBundleModelValidator()
    {
        RuleFor(x => x.BundleName).NotEmpty().WithMessage("Bundle name cannot be empty.");
        RuleFor(x => x.RetailPrice).NotEmpty().GreaterThanOrEqualTo(0)
            .WithMessage("Retail price must be non-negative.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Bundle items are required");
    }
}

public class UpdateBundleModelValidator : BaseBundleModelValidator<UpdateBundleModel>
{
    public UpdateBundleModelValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required.");
    }
}

public class CreateBundleModelValidator : BaseBundleModelValidator<CreateBundleModel>;