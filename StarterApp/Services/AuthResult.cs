using StarterApp.Database.Models;

namespace StarterApp.Services;

//represents the result of an authentication action
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public User? User { get; set; }
    public List<string> Roles { get; set; } = new();

    //creates a successful authentication result with the user and roles
    public static AuthResult Success(User user, List<string> roles)
    {
        return new AuthResult
        {
            IsSuccess = true,
            User = user,
            Roles = roles
        };
    }

    //creates a failed authentication result with an error message
    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}