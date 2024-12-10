using API.Db;
using API.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Utils;

public static class DbInitializer
{
    private static readonly string[] Roles = ["admin", "manager", "cashier"];

    public static async Task Initialize(
        IServiceProvider serviceProvider,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager
    )
    {
        var dbContext = serviceProvider.GetRequiredService<Context>();
        await dbContext.Database.MigrateAsync();

        await SeedRolesAndAdminUserAsync(roleManager, userManager);
    }

    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var role in Roles) options.AddPolicy(role, policy => policy.RequireRole(role));
        });
    }

    private static async Task SeedRolesAndAdminUserAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<UserEntity> userManager
    )
    {
        foreach (var role in Roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        const string adminEmail = "admin@oct.com";
        const string adminPassword = "Password!1";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new UserEntity
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Admin",
                IsDeleted = false
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
                throw new Exception(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );

            await userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}