using StarterApp.Models.Api;
using StarterApp.Repositories;

namespace StarterApp.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        return _itemRepository.GetItemsAsync(category, search, page, pageSize);
    }

    public Task<ItemDetailDto?> GetItemByIdAsync(int itemId)
    {
        return _itemRepository.GetItemByIdAsync(itemId);
    }

    public Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request)
    {
        return _itemRepository.CreateItemAsync(request);
    }

    public Task<(bool IsSuccess, string Message)> UpdateItemAsync(int itemId, CreateItemRequest request)
    {
        return _itemRepository.UpdateItemAsync(itemId, request);
    }

    public Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return _itemRepository.GetCategoriesAsync();
    }
}