// using API.Models;
// using Data.Entities.User;
// using Microsoft.AspNetCore.Mvc;

// namespace API.Controllers;

// [ApiController]
// [Route("api/auth")]
// public class UserController(ILogger<UserController> log) : ControllerBase
// {
//     [HttpPost("register")]
//     public async Task<IActionResult> Register([FromBody] RegisterModel model)
//     {
//         if (!ModelState.IsValid)
//             return BadRequest(ModelState);

//         var user = new UserEntity
//         {
//             UserName = model.Email,
//             Email = model.Email,
//             FirstName = model.FirstName,
//             LastName = model.LastName,
//         };

//         var result = await _userManager.CreateAsync(user, model.Password);
//         if (!result.Succeeded)
//             return BadRequest(result.Errors);

//         // Assign role
//         await _userManager.AddToRoleAsync(user, model.Role);

//         return Ok(new { Message = "User registered successfully" });
//     }
// }
