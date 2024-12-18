using API.Models.User;

namespace API.Services.User.Interfaces;

public interface IUserService
{
    Task<bool> RegisterUserAsync(RegisterModel model);
    Task<IResult> ChangeRoleAsync(ChangeRoleModel model);
    Task<bool> LoginUserAsync(LoginUserModel model);
    Task LogoutUserAsync();
}