/// @file MainViewModel.cs
/// @brief Main dashboard view model for authenticated users
/// @author StarterApp Development Team
/// @date 2025

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Views;
using StarterApp.Models.Api;
using StarterApp.Services;


namespace StarterApp.ViewModels;

/// @brief View model for the main dashboard page
/// @details Manages the main dashboard display, user information, and navigation to other sections
/// @extends BaseViewModel
public partial class MainViewModel : BaseViewModel
{
    /// @brief Authentication service for managing user authentication
    private readonly IAuthenticationService _authService;
    
    /// @brief Navigation service for managing page navigation
    private readonly INavigationService _navigationService;

    private readonly IItemService _itemService;

    /// @brief The currently authenticated user
    /// @details Observable property containing the current user's information
    [ObservableProperty]
    private User? currentUser;

    /// @brief Welcome message displayed to the user
    /// @details Observable property showing a personalized welcome message
    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    /// @brief Indicates whether the current user has admin privileges
    /// @details Observable property used to control visibility of admin features
    [ObservableProperty]
    private bool isAdmin;

    [ObservableProperty]
    private string itemsSummary = "Loading items...";

    public ObservableCollection<ItemSummaryDto> Items { get; } = new();
    
    /// @brief Initializes a new instance of the MainViewModel class
    /// @param authService The authentication service instance
    /// @param navigationService The navigation service instance
    /// @details Sets up the required services, initializes the title, and loads user data
    public MainViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        IItemService itemService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _itemService = itemService;

        Title = "Dashboard";

        LoadUserData();
        _ = RefreshDataAsync();
    }

    /// @brief Loads the current user's data and sets up the dashboard
    /// @details Retrieves current user information and determines admin status
    private void LoadUserData()
    {
        CurrentUser = _authService.CurrentUser;
        IsAdmin = _authService.HasRole("Admin");

        if (CurrentUser != null)
        {
            WelcomeMessage = $"Welcome, {CurrentUser.FullName}!";
        }
        else
        {
            WelcomeMessage = "Welcome!";
        }
    }

    /// @brief Logs out the current user
    /// @details Relay command that confirms logout and performs the logout operation
    /// @return A task representing the asynchronous logout operation
    [RelayCommand]
    private async Task LogoutAsync()
    {
        var result = await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (result)
        {
            await _authService.LogoutAsync();
            await _navigationService.NavigateToAsync("LoginPage");
        }
    }

    /// @brief Navigates to the user profile page
    /// @details Relay command that navigates to the profile page
    /// @return A task representing the asynchronous navigation operation
    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await _navigationService.NavigateToAsync(nameof(ProfilePage));
    }

    /// @brief Navigates to the settings page
    /// @details Relay command that navigates to the settings page
    /// @return A task representing the asynchronous navigation operation
    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await _navigationService.NavigateToAsync(nameof(SettingsPage));
    }


    /// @brief Navigates to the user list page
    /// @details Relay command that navigates to the user management page, admin only
    /// @return A task representing the asynchronous navigation operation
    [RelayCommand]
    private async Task NavigateToUserListAsync()
    {
        if (!IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Access Denied", "You don't have permission to access admin features.", "OK");
            return;
        }
        
        await _navigationService.NavigateToAsync("UserListPage");
    }

    /// @brief Refreshes the dashboard data
    /// @details Relay command that reloads user data and simulates a refresh operation
    /// @return A task representing the asynchronous refresh operation
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            LoadUserData();

            var result = await _itemService.GetItemsAsync(page: 1, pageSize: 20);

            Items.Clear();

            if (result?.Items != null)
            {
                foreach (var item in result.Items)
                {
                    Items.Add(item);
                }

                ItemsSummary = $"{result.TotalItems} item(s) available";
            }
            else
            {
                ItemsSummary = "Could not load items";
                SetError("Failed to load items from the API.");
            }
        }
        catch (Exception ex)
        {
            Items.Clear();
            ItemsSummary = "Could not load items";
            SetError($"Failed to refresh data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    //opens item details task
    [RelayCommand]
    private async Task OpenItemDetailAsync(ItemSummaryDto? item)
    {
        if (item is null)
        {
            return;
        }

        await _navigationService.NavigateToAsync(
            nameof(ItemDetailPage),
            new Dictionary<string, object>
            {
                { "ItemId", item.Id }
            });
    }

    [RelayCommand] //nav to create new item listing
    private async Task NavigateToCreateItemAsync()
    {
        await _navigationService.NavigateToAsync(nameof(CreateItemPage));
    }

    [RelayCommand] //opens rentals page
    private async Task NavigateToRentalsAsync()
    {
        await _navigationService.NavigateToAsync(nameof(RentalsPage));
    }
}