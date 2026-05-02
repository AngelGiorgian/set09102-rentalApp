using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models.Api;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IItemService _itemService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ItemDetailDto? item;

    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private string availabilityText = string.Empty;

    [ObservableProperty]
    private string ratingText = string.Empty;

    [ObservableProperty]
    private string ownerRatingText = string.Empty;

    public ItemDetailViewModel(IItemService itemService, INavigationService navigationService)
    {
        _itemService = itemService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ItemId", out var itemIdValue))
        {
            if (itemIdValue is int itemId)
            {
                _ = LoadItemAsync(itemId);
            }
            else if (itemIdValue is string itemIdText && int.TryParse(itemIdText, out var parsedId))
            {
                _ = LoadItemAsync(parsedId);
            }
        }
    }

    private async Task LoadItemAsync(int itemId)
    {
        try
        {
            IsBusy = true;
            ClearError();

            var result = await _itemService.GetItemByIdAsync(itemId);

            if (result is null)
            {
                SetError("Failed to load item details.");
                return;
            }

            Item = result;
            Title = result.Title;
            DailyRateText = result.DailyRateText;
            AvailabilityText = result.AvailabilityText;
            RatingText = result.RatingText;
            OwnerRatingText = result.OwnerRatingText;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item details: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}