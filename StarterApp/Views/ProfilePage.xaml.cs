/// @file ProfilePage.xaml.cs
/// @brief Code-behind for the profile page
/// @author StarterApp Development Team
/// @date 2025

using StarterApp.ViewModels;

namespace StarterApp.Views;

/// @brief Interaction logic for the ProfilePage
/// @details Sets the page binding context using dependency injection
public partial class ProfilePage : ContentPage
{
    /// @brief Initializes a new instance of the ProfilePage class
    /// @param viewModel The profile page view model
    /// @details Sets up the page and binds it to the injected view model
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}