using Microsoft.Extensions.DependencyInjection;

namespace BibleNotify;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		_services = services;
		InitializeComponent();
		UserAppTheme = Preferences.Get("app-theme", "System") switch
		{
			"Light" => AppTheme.Light,
			"System" => AppTheme.Unspecified,
			_ => AppTheme.Dark
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_services.GetRequiredService<AppShell>());
	}
}