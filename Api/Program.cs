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



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();