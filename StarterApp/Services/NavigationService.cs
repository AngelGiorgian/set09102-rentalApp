namespace StarterApp.Services;

//handles app navigation
public class NavigationService : INavigationService
{
    //goes to route
    public async Task NavigateToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    //goes to route with data
    public async Task NavigateToAsync(string route, Dictionary<string, object> parameters)
    {
        await Shell.Current.GoToAsync(route, parameters);
    }

    //goes back
    public async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    //goes to login root
    public async Task NavigateToRootAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    //clears navigation stack
    public async Task PopToRootAsync()
    {
        await Shell.Current.Navigation.PopToRootAsync();
    }
}