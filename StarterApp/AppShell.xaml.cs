/// @file AppShell.xaml.cs
/// @brief Application shell code-behind
/// @author StarterApp Development Team
/// @date 2025

using StarterApp.ViewModels;
using StarterApp.Views;

namespace StarterApp;

/// @brief Root shell for the application
/// @details Registers application routes and binds the shell view model
public partial class AppShell : Shell
{
    /// @brief Initializes a new instance of the AppShell class
    /// @param viewModel The app shell view model
    /// @details Binds the shell view model and registers navigation routes
    public AppShell(AppShellViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
        Routing.RegisterRoute(nameof(CreateItemPage), typeof(CreateItemPage));
        Routing.RegisterRoute(nameof(RentalsPage), typeof(RentalsPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}