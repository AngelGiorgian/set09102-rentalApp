using System.Net.Http.Json;
using System.Text.Json;
using StarterApp.Models.Api;

namespace StarterApp.Services;

public class ItemService : IItemService
{
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
}