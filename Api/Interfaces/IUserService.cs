using API.Models;
using Data.Entities.User;

namespace API.Interfaces;

public interface IUserService
{
    Task<bool> RegisterUserAsync(RegisterModel model);
    Task<IResult> ChangeRoleAsync(ChangeRoleModel model);
    Task<bool> LoginUserAsync(LoginUserModel model);
    Task LogoutUserAsync();
}
