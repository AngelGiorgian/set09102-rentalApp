/// @file SettingsPage.xaml.cs
/// @brief Code-behind for the settings page
/// @author StarterApp Development Team
/// @date 2025

using StarterApp.ViewModels;

namespace StarterApp.Views;

/// @brief Interaction logic for the SettingsPage
/// @details Sets the page binding context using dependency injection
public partial class SettingsPage : ContentPage
{
    /// @brief Initializes a new instance of the SettingsPage class
    /// @param viewModel The settings page view model
    /// @details Sets up the page and binds it to the injected view model
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}