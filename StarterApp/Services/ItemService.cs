using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using StarterApp.Models.Api;

namespace StarterApp.Services;

public class ItemService : IItemService
{
    private const string AuthTokenKey = "auth_token";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ItemService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var queryParts = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(category))
        {
            queryParts.Add($"category={Uri.EscapeDataString(category)}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParts.Add($"search={Uri.EscapeDataString(search)}");
        }

        var url = "/items";
        if (queryParts.Count > 0)
        {
            url += "?" + string.Join("&", queryParts);
        }

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ItemsResponse>(_jsonOptions);
    }

    public async Task<ItemDetailDto?> GetItemByIdAsync(int itemId)
    {
        using var response = await _httpClient.GetAsync($"/items/{itemId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ItemDetailDto>(_jsonOptions);
    }

    public async Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "You are not logged in.");
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/items");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Item created successfully.");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(errorBody))
            {
                return (false, errorBody);
            }

            return (false, $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return (false, $"Create item failed: {ex.Message}");
        }
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("/categories");

            if (!response.IsSuccessStatusCode)
            {
                return new List<CategoryDto>();
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<CategoryDto>();
            }

            using var document = JsonDocument.Parse(raw);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<CategoryDto>>(raw, _jsonOptions) ?? new List<CategoryDto>();
            }

            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("categories", out var categoriesElement))
            {
                return JsonSerializer.Deserialize<List<CategoryDto>>(categoriesElement.GetRawText(), _jsonOptions) ?? new List<CategoryDto>();
            }

            return new List<CategoryDto>();
        }
        catch
        {
            return new List<CategoryDto>();
        }
    }
}