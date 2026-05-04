using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

public sealed class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}

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

public sealed class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}