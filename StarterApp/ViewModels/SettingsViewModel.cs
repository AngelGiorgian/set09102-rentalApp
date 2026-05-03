/// @file SettingsViewModel.cs
/// @brief Application settings view model
/// @author StarterApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

/// @brief View model for the settings page
/// @details Displays simple application and session information and supports logout
/// @extends BaseViewModel
public partial class SettingsViewModel : BaseViewModel
{
    /// @brief Authentication service for user/session state
    private readonly IAuthenticationService _authService;

    /// @brief Navigation service for page navigation
    private readonly INavigationService _navigationService;

    /// @brief Display name of the application
    [ObservableProperty]
    private string appName = "StarterApp Rental Marketplace";

    /// @brief Base URL of the hosted coursework API
    [ObservableProperty]
    private string apiBaseUrl = "https://set09102-api.b-davison.workers.dev";

    /// @brief Current login status text
    [ObservableProperty]
    private string loggedInStatus = string.Empty;

    /// @brief Current logged-in user email
    [ObservableProperty]
    private string currentUserEmail = string.Empty;

    /// @brief Initializes a new instance of the SettingsViewModel class
    /// @param authService The authentication service instance
    /// @param navigationService The navigation service instance
    /// @details Sets the page title and loads current session details
    public SettingsViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Settings";

        LoadSettings();
    }

    /// @brief Loads the current settings/session information
    /// @details Populates the displayed app and authentication state values
    private void LoadSettings()
    {
        LoggedInStatus = _authService.IsAuthenticated ? "Logged in" : "Logged out";
        CurrentUserEmail = _authService.CurrentUser?.Email ?? "No active user";
    }

    /// @brief Refreshes the displayed settings/session information
    /// @details Reloads the current values from the authentication service
    /// @return A completed task after refreshing the data
    [RelayCommand]
    private Task RefreshAsync()
    {
        ClearError();
        LoadSettings();
        return Task.CompletedTask;
    }

    /// @brief Logs out the current user
    /// @details Confirms the action, then clears session state and returns to the login page
    /// @return A task representing the asynchronous logout operation
    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (!confirm)
        {
            return;
        }

        await _authService.LogoutAsync();
        await _navigationService.NavigateToAsync("LoginPage");
    }

    /// @brief Navigates back to the previous page
    /// @details Relay command that performs backward navigation
    /// @return A task representing the asynchronous navigation operation
    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}