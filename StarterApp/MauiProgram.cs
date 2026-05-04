using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Views;
using StarterApp.Services;
using StarterApp.Repositories;
using StarterApp.Security;

namespace StarterApp;

//configure the maui app
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

        builder.Services.AddDbContext<AppDbContext>(); //register db context

        builder.Services.AddSingleton(new HttpClient //register api client
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev"),
            Timeout = TimeSpan.FromSeconds(30)
        });

        //register token storage
        builder.Services.AddSingleton<ITokenProvider, MauiTokenProvider>();

        //register repos
        builder.Services.AddSingleton<IItemRepository, ItemRepository>();
        builder.Services.AddSingleton<IRentalRepository, RentalRepository>();

        //register services
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IRentalService, RentalService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        //registers app shell
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        //registers pages + viewmodels
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
        builder.Logging.AddDebug(); //debug logging
#endif

        return builder.Build();
    }
}