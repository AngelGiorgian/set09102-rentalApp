using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models.Api;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateItemViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IItemService _itemService;
    private readonly INavigationService _navigationService;

    private int? _editingItemId;

    [ObservableProperty]
    private string titleText = string.Empty;

    [ObservableProperty]
    private string descriptionText = string.Empty;

    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private CategoryDto? selectedCategory;

    [ObservableProperty]
    private string latitudeText = string.Empty;

    [ObservableProperty]
    private string longitudeText = string.Empty;

    [ObservableProperty]
    private bool areCategoriesLoaded;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string saveButtonText = "Create Item";

    public ObservableCollection<CategoryDto> Categories { get; } = new();

    public CreateItemViewModel(IItemService itemService, INavigationService navigationService)
    {
        _itemService = itemService;
        _navigationService = navigationService;
        Title = "Create Item";

        _ = LoadCategoriesAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ItemId", out var itemIdValue))
        {
            if (itemIdValue is int itemId)
            {
                _editingItemId = itemId;
                IsEditMode = true;
                Title = "Edit Item";
                SaveButtonText = "Update Item";
                _ = LoadExistingItemAsync(itemId);
            }
            else if (itemIdValue is string itemIdText && int.TryParse(itemIdText, out var parsedId))
            {
                _editingItemId = parsedId;
                IsEditMode = true;
                Title = "Edit Item";
                SaveButtonText = "Update Item";
                _ = LoadExistingItemAsync(parsedId);
            }
        }
        else
        {
            _editingItemId = null;
            IsEditMode = false;
            Title = "Create Item";
            SaveButtonText = "Create Item";
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            Categories.Clear();

            var categories = await _itemService.GetCategoriesAsync();

            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            AreCategoriesLoaded = Categories.Count > 0;

            if (!AreCategoriesLoaded)
            {
                SetError("Could not load categories from the API.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadExistingItemAsync(int itemId)
    {
        try
        {
            IsBusy = true;
            ClearError();

            if (Categories.Count == 0)
            {
                await LoadCategoriesAsync();
            }

            var item = await _itemService.GetItemByIdAsync(itemId);

            if (item is null)
            {
                SetError("Failed to load item details for editing.");
                return;
            }

            TitleText = item.Title;
            DescriptionText = item.Description;
            DailyRateText = item.DailyRate.ToString("0.##", CultureInfo.InvariantCulture);

            if (item.Latitude.HasValue)
            {
                LatitudeText = item.Latitude.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (item.Longitude.HasValue)
            {
                LongitudeText = item.Longitude.Value.ToString(CultureInfo.InvariantCulture);
            }

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item for editing: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            if (string.IsNullOrWhiteSpace(TitleText) ||
                string.IsNullOrWhiteSpace(DescriptionText) ||
                string.IsNullOrWhiteSpace(DailyRateText) ||
                SelectedCategory is null ||
                string.IsNullOrWhiteSpace(LatitudeText) ||
                string.IsNullOrWhiteSpace(LongitudeText))
            {
                SetError("Please fill in all fields and select a category.");
                return;
            }

            if (!decimal.TryParse(DailyRateText, NumberStyles.Number, CultureInfo.InvariantCulture, out var dailyRate))
            {
                SetError("Daily rate must be a valid number, for example 12.50");
                return;
            }

            if (!double.TryParse(LatitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
            {
                SetError("Latitude must be a valid number.");
                return;
            }

            if (!double.TryParse(LongitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            {
                SetError("Longitude must be a valid number.");
                return;
            }

            var request = new CreateItemRequest
            {
                Title = TitleText.Trim(),
                Description = DescriptionText.Trim(),
                DailyRate = dailyRate,
                CategoryId = SelectedCategory.Id,
                Latitude = latitude,
                Longitude = longitude
            };

            (bool IsSuccess, string Message) result;

            if (IsEditMode && _editingItemId.HasValue)
            {
                result = await _itemService.UpdateItemAsync(_editingItemId.Value, request);
            }
            else
            {
                result = await _itemService.CreateItemAsync(request);
            }

            if (!result.IsSuccess)
            {
                SetError(result.Message);
                return;
            }

            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Success",
                IsEditMode ? "Item updated successfully." : "Item created successfully.",
                "OK");

            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    [RelayCommand]
    private async Task ReloadCategoriesAsync()
    {
        await LoadCategoriesAsync();

        if (IsEditMode && _editingItemId.HasValue)
        {
            await LoadExistingItemAsync(_editingItemId.Value);
        }
    }
}