using FluentValidation;

namespace API.Filters;

public class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var request = context.Arguments.OfType<TRequest>().First();
        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid) return TypedResults.ValidationProblem(result.ToDictionary());
        return await next(context);
    }
}

//works for list but i dont need it for now
// using FluentValidation;

// namespace API.Filters;

// public class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
// {
//     private readonly IValidator<TRequest> _validator = validator;

//     public async ValueTask<object?> InvokeAsync(
//         EndpointFilterInvocationContext context,
//         EndpointFilterDelegate next
//     )
//     {
//         var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
//         var requestList = context.Arguments.OfType<IEnumerable<TRequest>>().FirstOrDefault();

//         if (request != null)
//         {
//             // Single object validation
//             var result = await _validator.ValidateAsync(
//                 request,
//                 context.HttpContext.RequestAborted
//             );
//             if (!result.IsValid)
//             {
//                 return TypedResults.ValidationProblem(result.ToDictionary());
//             }
//         }
//         else if (requestList != null)
//         {
//             // List validation
//             var validationErrors = new Dictionary<string, string[]>();

//             foreach (var item in requestList)
//             {
//                 var result = await _validator.ValidateAsync(
//                     item,
//                     context.HttpContext.RequestAborted
//                 );
//                 if (!result.IsValid)
//                 {
//                     foreach (var failure in result.Errors)
//                     {
//                         if (!validationErrors.ContainsKey(failure.PropertyName))
//                         {
//                             validationErrors[failure.PropertyName] = new List<string>().ToArray();
//                         }

//                         validationErrors[failure.PropertyName] = validationErrors[
//                             failure.PropertyName
//                         ]
//                             .Append(failure.ErrorMessage)
//                             .ToArray();
//                     }
//                 }
//             }

//             if (validationErrors.Any())
//             {
//                 return TypedResults.ValidationProblem(validationErrors);
//             }
//         }
//         else
//         {
//             return TypedResults.BadRequest("Request is invalid or unsupported.");
//         }

//         // Proceed to the next middleware or endpoint handler
//         return await next(context);
//     }
// }