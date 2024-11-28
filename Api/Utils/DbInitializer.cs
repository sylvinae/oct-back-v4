using Data.Db;
using Data.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Utils;

public static class DbInitializer
{
    private static readonly string[] Roles = ["admin", "manager", "cashier"];

    /// <summary>
    /// Initializes the database, applies migrations, and seeds roles and admin user.
    /// </summary>
    public static async Task Initialize(
        IServiceProvider serviceProvider,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager
    )
    {
        // Apply migrations automatically
        var dbContext = serviceProvider.GetRequiredService<Context>();
        await dbContext.Database.MigrateAsync();

        // Seed roles and admin user
        await SeedRolesAndAdminUserAsync(roleManager, userManager);
    }

    /// <summary>
    /// Adds authorization policies for predefined roles.
    /// </summary>
    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var role in Roles)
            {
                options.AddPolicy(role, policy => policy.RequireRole(role));
            }
        });
    }

    /// <summary>
    /// Seeds predefined roles and admin user into the database.
    /// </summary>
    private static async Task SeedRolesAndAdminUserAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<UserEntity> userManager
    )
    {
        // Seed roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@oct.com";
        var adminPassword = "Password!1";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new UserEntity
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin", // Provide default FirstName
                LastName = "Admin", // Provide default LastName
                IsDeleted = false,
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            await userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}
