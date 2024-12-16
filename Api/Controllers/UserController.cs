using API.Models.User;
using API.Services.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/auth")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
    {
        var success = await userService.RegisterUserAsync(model);
        if (!success) return BadRequest(new { message = "Registration failed" });

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserModel model)
    {
        var success = await userService.LoginUserAsync(model);
        if (!success) return BadRequest(new { message = "Invalid credentials" });

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutUser()
    {
        await userService.LogoutUserAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("change-role")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleModel model)
    {
        var result = await userService.ChangeRoleAsync(model);
        return Ok(result);
    }
}