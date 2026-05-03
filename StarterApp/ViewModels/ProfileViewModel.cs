/// @file ProfileViewModel.cs
/// @brief User profile display view model
/// @author StarterApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

/// @brief View model for the user profile page
/// @details Displays the currently logged-in user's basic profile information
/// @extends BaseViewModel
public partial class ProfileViewModel : BaseViewModel
{
    /// @brief Authentication service for retrieving current user information
    private readonly IAuthenticationService _authService;

    /// @brief Navigation service for page navigation
    private readonly INavigationService _navigationService;

    /// @brief The current authenticated user
    /// @details Holds the current user's raw model data
    [ObservableProperty]
    private User? currentUser;

    /// @brief The user's full name
    /// @details Formatted from first name and last name
    [ObservableProperty]
    private string fullName = string.Empty;

    /// @brief The user's email address
    /// @details Displayed on the profile page
    [ObservableProperty]
    private string email = string.Empty;

    /// @brief The user's role list as text
    /// @details Comma-separated role names for display
    [ObservableProperty]
    private string rolesText = string.Empty;

    /// @brief The user's authentication/account status text
    /// @details Indicates whether the user is authenticated
    [ObservableProperty]
    private string accountStatusText = string.Empty;

    /// @brief Initializes a new instance of the ProfileViewModel class
    /// @param authService The authentication service instance
    /// @param navigationService The navigation service instance
    /// @details Sets the page title and loads the user profile data
    public ProfileViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Profile";

        LoadUserData();
    }

    /// @brief Loads the current user's profile data
    /// @details Reads the logged-in user from the authentication service and formats display fields
    private void LoadUserData()
    {
        CurrentUser = _authService.CurrentUser;

        if (CurrentUser is null)
        {
            FullName = "Unknown User";
            Email = "Not available";
            RolesText = "No roles";
            AccountStatusText = "Not authenticated";
            return;
        }

        FullName = $"{CurrentUser.FirstName} {CurrentUser.LastName}".Trim();
        Email = CurrentUser.Email;
        RolesText = _authService.CurrentUserRoles.Count > 0
            ? string.Join(", ", _authService.CurrentUserRoles)
            : "No roles";
        AccountStatusText = _authService.IsAuthenticated ? "Authenticated" : "Not authenticated";
    }

    /// @brief Refreshes the profile information
    /// @details Reloads the current user's display data from the authentication service
    /// @return A completed task after refreshing the data
    [RelayCommand]
    private Task RefreshAsync()
    {
        ClearError();
        LoadUserData();
        return Task.CompletedTask;
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