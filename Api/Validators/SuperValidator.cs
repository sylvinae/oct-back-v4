using FluentValidation;

namespace API.Validators;

public class SuperValidator
{
    public static async Task<(bool IsValid, string? ErrorMessage)> Check<T>(
        IValidator<T> validator,
        T model
    )
    {
        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            var errorMessage = string.Join(" ", result.Errors.Select(e => e.ErrorMessage));

            return (false, errorMessage);
        }

        return (true, null);
    }
}
