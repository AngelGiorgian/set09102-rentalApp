using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models.Api;
using StarterApp.Services;
using StarterApp.Views;

namespace StarterApp.ViewModels;

//handles item details screen logic
public partial class ItemDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IItemService _itemService;
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
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

    [ObservableProperty]
    private DateTime rentalStartDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime rentalEndDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private bool canRequestRental;

    [ObservableProperty]
    private string rentalInfoMessage = string.Empty;

    [ObservableProperty]
    private bool isOwner;

    public ItemDetailViewModel(
        IItemService itemService,
        IRentalService rentalService,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemService = itemService;
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    //gets item id from navigation
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

    //loads item details
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

            //checks ownership
            var currentUserId = _authService.CurrentUser?.Id;
            var isOwnItem = currentUserId.HasValue && currentUserId.Value == result.OwnerId;

            IsOwner = isOwnItem;
            CanRequestRental = result.IsAvailable && !isOwnItem;

            //sets rental message
            if (!result.IsAvailable)
            {
                RentalInfoMessage = "This item is currently unavailable for rental.";
            }
            else if (isOwnItem)
            {
                RentalInfoMessage = "You own this item. You can edit it below.";
            }
            else
            {
                RentalInfoMessage = "Select your rental dates and submit a request.";
            }
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

    //opens edit item page
    [RelayCommand]
    private async Task NavigateToEditItemAsync()
    {
        if (Item is null || !IsOwner)
        {
            return;
        }

        await _navigationService.NavigateToAsync(
            nameof(CreateItemPage),
            new Dictionary<string, object>
            {
                { "ItemId", Item.Id }
            });
    }

    //submits rental request
    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        try
        {
            ClearError();

            if (Item is null)
            {
                SetError("No item is loaded.");
                return;
            }

            if (!CanRequestRental)
            {
                SetError("This item cannot be requested.");
                return;
            }

            var startDate = RentalStartDate.Date;
            var endDate = RentalEndDate.Date;

            //checks start date
            if (startDate < DateTime.Today)
            {
                SetError("Start date cannot be in the past.");
                return;
            }

            //checks end date
            if (endDate <= startDate)
            {
                SetError("End date must be after the start date.");
                return;
            }

            IsBusy = true;

            //builds rental request
            var request = new CreateRentalRequest
            {
                ItemId = Item.Id,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };

            var result = await _rentalService.RequestRentalAsync(request);

            if (!result.IsSuccess)
            {
                SetError(result.Message);
                return;
            }

            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Success",
                "Rental request submitted successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit rental request: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    //goes back
    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}