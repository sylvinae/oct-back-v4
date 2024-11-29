using System.Reflection;
using API.Endpoints;
using API.Interfaces;
using API.Models.Item;
using API.Services;
using API.Services.User;
using API.Utils;
using API.Validators;
using Data.Db;
using Data.Entities.User;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddCustomServices(Assembly.GetExecutingAssembly());
builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication();

builder
    .Services.AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Tokens.AuthenticatorTokenProvider = "";
    })
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>();
builder.Services.AddAuthorizationPolicies();
var app = builder.Build();

// Enable Swagger in development
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }


// Optionally add exception handler middleware for general exception handling
app.UseExceptionHandler("/error"); // Redirects to a global error handler

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
app.MapErrorEndpoints();

// Middleware
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
