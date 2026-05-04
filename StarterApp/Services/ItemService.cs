using StarterApp.Models.Api;
using StarterApp.Repositories;

namespace StarterApp.Services;

//handles item actions
public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    //gets item list
    public Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        return _itemRepository.GetItemsAsync(category, search, page, pageSize);
    }

    //gets item details
    public Task<ItemDetailDto?> GetItemByIdAsync(int itemId)
    {
        return _itemRepository.GetItemByIdAsync(itemId);
    }

    //creates item
    public Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request)
    {
        return _itemRepository.CreateItemAsync(request);
    }

    //updates item
    public Task<(bool IsSuccess, string Message)> UpdateItemAsync(int itemId, CreateItemRequest request)
    {
        return _itemRepository.UpdateItemAsync(itemId, request);
    }

    //gets categories
    public Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return _itemRepository.GetCategoriesAsync();
    }
}