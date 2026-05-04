namespace StarterApp.Services;

//navigation service contract
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, Dictionary<string, object> parameters); //goes to route with data
    Task NavigateBackAsync();
    Task NavigateToRootAsync();
    Task PopToRootAsync();
}