namespace API.Models.User;

public class BaseUserModel
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
}

public class LoginUserModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterModel : BaseUserModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
}

public class ChangeRoleModel
{
    public Guid UserId { get; set; }
    public required string NewRole { get; set; }
}