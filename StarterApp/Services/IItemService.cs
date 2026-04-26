using StarterApp.Models.Api;

namespace StarterApp.Services;

public interface IItemService
{
    Task<ItemsResponse?> GetItemsAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 20);
}