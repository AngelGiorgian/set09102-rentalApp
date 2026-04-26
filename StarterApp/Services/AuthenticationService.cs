using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;
using BCrypt.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using StarterApp.Models.Api;

namespace StarterApp.Services;

public class AuthenticationService : IAuthenticationService
{
    private const string AuthTokenKey = "auth_token";
    private const string AuthTokenExpiresAtKey = "auth_token_expires_at";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private User? _currentUser;
    private List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        //try to restore a previous session when the service is created
        _ = TryRestoreSessionAsync();
    }

    public bool IsAuthenticated => _currentUser is not null;

    public User? CurrentUser => _currentUser;

    public List<string> CurrentUserRoles => _currentUserRoles;

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email.Trim(),
                Password = password
            };

            using var response = await _httpClient.PostAsync(
                "/auth/token",
                CreateJsonContent(request));

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await ReadApiErrorAsync(response);
                return new AuthenticationResult(false, errorMessage);
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);

            if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.Token))
            {
                return new AuthenticationResult(false, "Login failed: token was not returned by the API.");
            }

            await SaveTokenAsync(loginResponse.Token, loginResponse.ExpiresAt);

            var currentUser = await GetCurrentUserAsync(loginResponse.Token);
            if (currentUser is null)
            {
                await ClearSessionAsync(false);
                return new AuthenticationResult(false, "Login failed: could not load current user.");
            }

            SetAuthenticatedUser(currentUser, loginResponse.Token);

            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var request = new RegisterRequest
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim(),
                Password = password
            };

            using var response = await _httpClient.PostAsync(
                "/auth/register",
                CreateJsonContent(request));

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await ReadApiErrorAsync(response);
                return new AuthenticationResult(false, errorMessage);
            }

            return new AuthenticationResult(true, "Registration successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        return ClearSessionAsync(true);
    }

    public bool HasRole(string roleName)
    {
        return _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyRole(params string[] roleNames)
    {
        return roleNames.Any(HasRole);
    }

    public bool HasAllRoles(params string[] roleNames)
    {
        return roleNames.All(HasRole);
    }

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        return Task.FromResult(false);
    }

    private async Task TryRestoreSessionAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var expiresAtRaw = await SecureStorage.Default.GetAsync(AuthTokenExpiresAtKey);
            if (DateTime.TryParse(expiresAtRaw, out var expiresAt) && expiresAt <= DateTime.UtcNow)
            {
                await ClearSessionAsync(false);
                return;
            }

            var currentUser = await GetCurrentUserAsync(token);
            if (currentUser is null)
            {
                await ClearSessionAsync(false);
                return;
            }

            SetAuthenticatedUser(currentUser, token);
        }
        catch
        {
            await ClearSessionAsync(false);
        }
    }

    private async Task<User?> GetCurrentUserAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var apiUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(_jsonOptions);
        if (apiUser is null)
        {
            return null;
        }

        return new User
        {
            Id = apiUser.Id,
            FirstName = apiUser.FirstName,
            LastName = apiUser.LastName,
            Email = apiUser.Email,
            PasswordHash = string.Empty,
            PasswordSalt = string.Empty,
            CreatedAt = apiUser.RegisteredAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    private void SetAuthenticatedUser(User user, string token)
    {
        _currentUser = user;
        _currentUserRoles = ExtractRolesFromToken(token);
        AuthenticationStateChanged?.Invoke(this, true);
    }

    private async Task SaveTokenAsync(string token, DateTime? expiresAt)
    {
        await SecureStorage.Default.SetAsync(AuthTokenKey, token);

        if (expiresAt.HasValue)
        {
            await SecureStorage.Default.SetAsync(
                AuthTokenExpiresAtKey,
                expiresAt.Value.ToString("O"));
        }
        else
        {
            SecureStorage.Default.Remove(AuthTokenExpiresAtKey);
        }
    }

    private Task ClearSessionAsync(bool raiseEvent)
    {
        _currentUser = null;
        _currentUserRoles.Clear();

        SecureStorage.Default.Remove(AuthTokenKey);
        SecureStorage.Default.Remove(AuthTokenExpiresAtKey);

        if (raiseEvent)
        {
            AuthenticationStateChanged?.Invoke(this, false);
        }

        return Task.CompletedTask;
    }

    private static StringContent CreateJsonContent<T>(T value)
    {
        return new StringContent(
            JsonSerializer.Serialize(value),
            Encoding.UTF8,
            "application/json");
    }

    private async Task<string> ReadApiErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}";
            }

            var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(raw, _jsonOptions);

            if (!string.IsNullOrWhiteSpace(apiError?.Message))
            {
                return apiError.Message!;
            }

            if (!string.IsNullOrWhiteSpace(apiError?.Error))
            {
                return apiError.Error!;
            }

            return raw;
        }
        catch
        {
            return $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}";
        }
    }

    private static List<string> ExtractRolesFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return new List<string>();
            }

            var payloadJson = DecodeBase64Url(parts[1]);
            using var document = JsonDocument.Parse(payloadJson);

            var roles = new List<string>();

            AddRoles(document.RootElement, "role", roles);
            AddRoles(document.RootElement, "roles", roles);
            AddRoles(document.RootElement, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", roles);
            AddRoles(document.RootElement, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role", roles);

            return roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static void AddRoles(JsonElement root, string propertyName, List<string> roles)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var value = property.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                roles.Add(value);
            }

            return;
        }

        if (property.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    roles.Add(value);
                }
            }
        }
    }

    private static string DecodeBase64Url(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');

        switch (output.Length % 4)
        {
            case 2:
                output += "==";
                break;
            case 3:
                output += "=";
                break;
        }

        var bytes = Convert.FromBase64String(output);
        return Encoding.UTF8.GetString(bytes);
    }
}

public class AuthenticationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }

    public AuthenticationResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}