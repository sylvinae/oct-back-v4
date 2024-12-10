using API.Entities.User;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Services.User;

public class UserService(
    UserManager<UserEntity> _userManager,
    RoleManager<IdentityRole<Guid>> _roleManager,
    SignInManager<UserEntity> _signInManager
) : IUserService
{
    public async Task<bool> RegisterUserAsync(RegisterModel model)
    {
        var user = new UserEntity
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return false;

        var role = await _roleManager.FindByNameAsync(model.Role);
        if (role != null && !string.IsNullOrEmpty(role.Name))
        {
            await _userManager.AddToRoleAsync(user, role.Name);
        }
        else
            return false;

        return true;
    }

    public async Task<bool> LoginUserAsync(LoginUserModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return false;

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            isPersistent: true,
            lockoutOnFailure: false
        );

        return signInResult.Succeeded;
    }

    public async Task LogoutUserAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<IResult> ChangeRoleAsync(ChangeRoleModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
        {
            return Results.BadRequest("User not found");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(model.NewRole))
        {
            return Results.BadRequest("User already has the specified role.");
        }

        foreach (var r in currentRoles)
        {
            await _userManager.RemoveFromRoleAsync(user, r);
        }

        var role = await _roleManager.FindByNameAsync(model.NewRole);
        if (role == null)
        {
            return Results.BadRequest("Role not found");
        }

        await _userManager.AddToRoleAsync(user, model.NewRole);

        return Results.Ok(new { message = "User role changed successfully" });
    }
}
