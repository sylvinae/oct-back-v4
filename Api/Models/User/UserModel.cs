using Newtonsoft.Json;

namespace API.Models.User;

public class BaseUserModel
{
    [JsonProperty("firstName")] public required string FirstName { get; set; }

    [JsonProperty("middleName")] public string? MiddleName { get; set; }

    [JsonProperty("lastName")] public required string LastName { get; set; }
}

public class LoginUserModel
{
    [JsonProperty("email")] public required string Email { get; set; }

    [JsonProperty("password")] public required string Password { get; set; }
}

public class ResponseLoginUserModel : BaseUserModel
{
    [JsonProperty("email")] public required string Email { get; set; }

    [JsonProperty("token")] public required string Token { get; set; }

    [JsonProperty("role")] public required string Roles { get; set; }
}

public class RegisterModel : BaseUserModel
{
    [JsonProperty("email")] public required string Email { get; set; }

    [JsonProperty("password")] public required string Password { get; set; }

    [JsonProperty("role")] public required string Role { get; set; }
}

public class ChangeRoleModel
{
    [JsonProperty("userId")] public Guid UserId { get; set; }

    [JsonProperty("newRole")] public required string NewRole { get; set; }
}