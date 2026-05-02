using System.Globalization;
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

    public Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync()
    {
        return GetRentalsAsync("/rentals/outgoing");
    }

    public Task<List<RentalSummaryDto>> GetIncomingRentalsAsync()
    {
        return GetRentalsAsync("/rentals/incoming");
    }

    private async Task<List<RentalSummaryDto>> GetRentalsAsync(string url)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new List<RentalSummaryDto>();
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new List<RentalSummaryDto>();
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<RentalSummaryDto>();
            }

            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.EnumerateArray().Select(MapRental).ToList();
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var propertyName in new[] { "rentals", "items", "data" })
                {
                    if (root.TryGetProperty(propertyName, out var arrayElement) &&
                        arrayElement.ValueKind == JsonValueKind.Array)
                    {
                        return arrayElement.EnumerateArray().Select(MapRental).ToList();
                    }
                }
            }

            return new List<RentalSummaryDto>();
        }
        catch
        {
            return new List<RentalSummaryDto>();
        }
    }

    private static RentalSummaryDto MapRental(JsonElement rental)
    {
        var dto = new RentalSummaryDto
        {
            Id = ReadInt(rental, "id"),
            ItemId = ReadInt(rental, "itemId"),
            Status = ReadString(rental, "status"),
            StartDate = ReadDate(rental, "startDate"),
            EndDate = ReadDate(rental, "endDate"),
            RequestedAt = ReadDate(rental, "requestedAt"),
            TotalPrice = ReadDecimalNullable(rental, "totalPrice"),
            BorrowerName = ReadString(rental, "borrowerName"),
            OwnerName = ReadString(rental, "ownerName"),
            ItemTitle = ReadString(rental, "itemTitle")
        };

        if (rental.TryGetProperty("item", out var itemElement) &&
            itemElement.ValueKind == JsonValueKind.Object)
        {
            if (dto.ItemId == 0)
            {
                dto.ItemId = ReadInt(itemElement, "id");
            }

            if (string.IsNullOrWhiteSpace(dto.ItemTitle))
            {
                dto.ItemTitle = ReadString(itemElement, "title");
            }
        }

        if (rental.TryGetProperty("borrower", out var borrowerElement) &&
            borrowerElement.ValueKind == JsonValueKind.Object &&
            string.IsNullOrWhiteSpace(dto.BorrowerName))
        {
            dto.BorrowerName = BuildFullName(borrowerElement);
        }

        if (rental.TryGetProperty("owner", out var ownerElement) &&
            ownerElement.ValueKind == JsonValueKind.Object &&
            string.IsNullOrWhiteSpace(dto.OwnerName))
        {
            dto.OwnerName = BuildFullName(ownerElement);
        }

        return dto;
    }

    private static string BuildFullName(JsonElement person)
    {
        var firstName = ReadString(person, "firstName");
        var lastName = ReadString(person, "lastName");
        var fullName = $"{firstName} {lastName}".Trim();

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        return ReadString(person, "name");
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : property.ToString();
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static DateTime? ReadDate(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static decimal? ReadDecimalNullable(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String &&
            decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
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