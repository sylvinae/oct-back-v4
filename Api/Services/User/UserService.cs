using API.Entities.User;
using API.Interfaces;
using API.Models.User;
using Microsoft.AspNetCore.Identity;

namespace API.Services.User;

public class UserService(
    UserManager<UserEntity> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    SignInManager<UserEntity> signInManager
) : IUserService
{
    public async Task<bool> RegisterUserAsync(RegisterModel model)
    {
        var user = new UserEntity
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return false;

        var role = await roleManager.FindByNameAsync(model.Role);
        if (role != null && !string.IsNullOrEmpty(role.Name))
            await userManager.AddToRoleAsync(user, role.Name);
        else
            return false;

        return true;
    }

    public async Task<bool> LoginUserAsync(LoginUserModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return false;

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            model.Password,
            true,
            false
        );

        return signInResult.Succeeded;
    }

    public async Task LogoutUserAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<IResult> ChangeRoleAsync(ChangeRoleModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null) return Results.BadRequest("User not found");

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(model.NewRole)) return Results.BadRequest("User already has the specified role.");

        foreach (var r in currentRoles) await userManager.RemoveFromRoleAsync(user, r);

        var role = await roleManager.FindByNameAsync(model.NewRole);
        if (role == null) return Results.BadRequest("Role not found");

        await userManager.AddToRoleAsync(user, model.NewRole);

        return Results.Ok(new { message = "User role changed successfully" });
    }
}