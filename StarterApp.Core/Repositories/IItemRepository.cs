using StarterApp.Models.Api;

namespace StarterApp.Repositories;

//item repository contract
public interface IItemRepository : IRepository<ItemSummaryDto>
{
    //gets item list
    Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20);

    Task<ItemDetailDto?> GetItemByIdAsync(int itemId); //item details

    Task<(bool IsSuccess, string Message)> CreateItemAsync(CreateItemRequest request); //create item
    Task<(bool IsSuccess, string Message)> UpdateItemAsync(int itemId, CreateItemRequest request); //update items

    Task<List<CategoryDto>> GetCategoriesAsync(); //categories
}