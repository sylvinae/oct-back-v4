using System.Reflection;
using API.Endpoints;
using API.Interfaces;
using API.Services.User;
using Api.Utils;
using API.Utils;
using Data.Db;
using Data.Entities.User;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddCustomServices(Assembly.GetExecutingAssembly());
builder.Services.AddAuthorization();

// builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

// builder.Services.AddAuthentication();

builder
    .Services.AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Tokens.AuthenticatorTokenProvider = "";
    })
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

// Register services


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>();
builder.Services.AddAuthorizationPolicies();
var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize database and seed roles/users
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await DbInitializer.Initialize(serviceProvider, userManager, roleManager);
}

// Map endpoints
app.MapUserEndpoints();
app.MapItemEndpoints();

// Middleware
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
