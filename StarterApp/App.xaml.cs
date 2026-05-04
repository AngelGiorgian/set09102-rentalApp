using StarterApp.ViewModels;

namespace StarterApp;

//main app setup
public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;
	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		InitializeComponent();
		//app routess
		Routing.RegisterRoute(nameof(Views.MainPage), typeof(Views.MainPage));
		Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
		Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
		Routing.RegisterRoute(nameof(Views.UserListPage), typeof(Views.UserListPage));
		Routing.RegisterRoute(nameof(Views.UserDetailPage), typeof(Views.UserDetailPage));
		Routing.RegisterRoute(nameof(Views.TempPage), typeof(Views.TempPage));
	}

	protected override Window CreateWindow(IActivationState? activationState) //creates main app window
	{
		// var window = base.CreateWindow(activationState);
		// window.Page = new AppShell();

		var shell = _serviceProvider.GetService<AppShell>();
		if (shell == null)
		{
			//handle the error if appshell not resolved
			throw new InvalidOperationException("AppShell could not be resolved from the service provider.");
		}
		var window = new Window(shell);
		return window;
	}
}
