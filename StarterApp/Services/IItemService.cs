using StarterApp.Models.Api;

namespace StarterApp.Services;

//item service contract
public interface IItemService
{
    //gets item list
    Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20);

    //gets item details
    Task<ItemDetailDto?> GetItemByIdAsync(int itemId);

    //creates item
    Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request);

    //updates item
    Task<(bool IsSuccess, string Message)> UpdateItemAsync(int itemId, CreateItemRequest request);

    //gets categories
    Task<List<CategoryDto>> GetCategoriesAsync();
}