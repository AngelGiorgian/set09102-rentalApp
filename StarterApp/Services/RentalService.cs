using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using StarterApp.Models.Api;

namespace StarterApp.Services;

public class RentalService : IRentalService
{
    private const string AuthTokenKey = "auth_token";

    private readonly HttpClient _httpClient;

    public RentalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "You are not logged in.");
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/rentals");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Rental request submitted successfully.");
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                return (false, ExtractErrorMessage(raw));
            }

            return (false, $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return (false, $"Rental request failed: {ex.Message}");
        }
    }

    private static string ExtractErrorMessage(string raw)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (root.TryGetProperty("message", out var messageElement) &&
                messageElement.ValueKind == JsonValueKind.String)
            {
                return messageElement.GetString() ?? raw;
            }

            if (root.TryGetProperty("error", out var errorElement) &&
                errorElement.ValueKind == JsonValueKind.String)
            {
                return errorElement.GetString() ?? raw;
            }

            return raw;
        }
        catch
        {
            return raw;
        }
    }
}