using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

//data sent to the API when a user logs in
public sealed class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

//data returned by the API after a successful login
public sealed class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}

//data sent to the API when creating a new user account
public sealed class RegisterRequest
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

//currently authenticated users profile returned by the API
public sealed class CurrentUserResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("registeredAt")]
    public DateTime? RegisteredAt { get; set; }
}

//standard error response returned by the API
public sealed class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}