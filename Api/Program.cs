using System.Reflection;
using System.Text.Json;
using API.Db;
using API.Entities.User;
using API.Utils;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add necessary services
builder.Services.AddLogging();
builder.Services.AddServicesByConvention(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<Context>();
builder.Services.AddAuthorizationPolicies();


builder
    .Services.AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Tokens.AuthenticatorTokenProvider = "";
    })
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<DataProtectorTokenProvider<UserEntity>>(TokenOptions.DefaultProvider);

// Enable Security Stamp Validation
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});
builder
    .Services.AddDataProtection()
    .SetApplicationName("PharmacyAppAPI")
    .UseEphemeralDataProtectionProvider();

// Add OData support
builder
    .Services.AddControllers()
    .AddOData(opt => opt.Select().Expand().Filter().OrderBy().Count().SetMaxTop(100))
    .AddJsonOptions(options => { options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; });

var app = builder.Build();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await DbInitializer.Initialize(serviceProvider, userManager, roleManager);
}


// app.Use(async (context, next) =>
// {
//     try
//     {
//         await next();
//
//         // If the response status code is 400, handle it
//         if (context.Response.StatusCode == StatusCodes.Status400BadRequest && !context.Response.HasStarted)
//         {
//             var problemDetails = new ProblemDetails
//             {
//                 Title = "One or more validation errors occurred. or two",
//                 Status = StatusCodes.Status400BadRequest,
//                 Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
//                 Instance = context.Request.Path,
//                 Detail = "Validation errors occurred."
//             };
//
//             // Check if ModelState is available
//             if (context.Items.ContainsKey("ModelState"))
//             {
//                 var modelState = context.Items["ModelState"] as ModelStateDictionary;
//                 if (modelState != null)
//                 {
//                     // Create a simple string representation of the errors
//                     var errors = modelState
//                         .Where(ms => ms.Value.Errors.Count > 0)
//                         .Select(ms => new
//                         {
//                             Field = ms.Key,
//                             Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
//                         })
//                         .ToArray();
//
//                     // Serialize the errors to JSON and set it in the Detail property
//                     problemDetails.Detail = JsonSerializer.Serialize(errors);
//                 }
//             }
//
//             context.Response.ContentType = "application/json";
//             await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
//         }
//     }
//     catch (Exception ex)
//     {
//         // Handle unexpected exceptions
//         if (!context.Response.HasStarted)
//         {
//             context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//             context.Response.ContentType = "application/json";
//             var errorResponse = new ProblemDetails
//             {
//                 Title = "An unexpected error occurred.",
//                 Status = StatusCodes.Status500InternalServerError,
//                 Detail = ex.Message,
//                 Instance = context.Request.Path
//             };
//             await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
//         }
//     }
// });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();