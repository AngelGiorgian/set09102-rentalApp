using StarterApp.Models.Api;

namespace StarterApp.Services;

public interface IItemService
{
    Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20);

    Task<ItemDetailDto?> GetItemByIdAsync(int itemId);

    Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request);
    Task<(bool IsSuccess, string Message)> UpdateItemAsync(int itemId, CreateItemRequest request);

    Task<List<CategoryDto>> GetCategoriesAsync();
}