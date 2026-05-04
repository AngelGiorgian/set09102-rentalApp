using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Views;
using StarterApp.Services;
using StarterApp.Repositories;
using StarterApp.Security;

namespace StarterApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddDbContext<AppDbContext>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev"),
            Timeout = TimeSpan.FromSeconds(30)
        });

        builder.Services.AddSingleton<ITokenProvider, MauiTokenProvider>();

        builder.Services.AddSingleton<IItemRepository, ItemRepository>();
        builder.Services.AddSingleton<IRentalRepository, RentalRepository>();

        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IRentalService, RentalService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}