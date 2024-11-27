using API.Interfaces;
using API.Models;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth");

        authGroup.MapPost("/register", RegisterUser).RequireAuthorization("admin");
        authGroup.MapPost("/login", LoginUser);
        authGroup.MapPost("/logout", LogoutUser);
        authGroup.MapPost("/change-role", ChangeRole).RequireAuthorization("admin");
    }

    private static async Task<IResult> RegisterUser(RegisterModel model, IUserService userService)
    {
        var success = await userService.RegisterUserAsync(model);
        if (!success)
        {
            return Results.BadRequest(new { message = "Registration failed" });
        }

        return Results.Ok(new { message = "User registered successfully" });
    }

    private static async Task<IResult> LoginUser(LoginUserModel model, IUserService userService)
    {
        var success = await userService.LoginUserAsync(model);
        if (!success)
        {
            return Results.BadRequest(new { message = "Invalid credentials" });
        }

        return Results.Ok(new { message = "Login successful" });
    }

    private static async Task<IResult> LogoutUser(IUserService userService)
    {
        await userService.LogoutUserAsync();
        return Results.Ok(new { message = "Logged out successfully" });
    }

    public static async Task<IResult> ChangeRole(ChangeRoleModel model, IUserService userService)
    {
        return await userService.ChangeRoleAsync(model);
    }
}
