using Microsoft.Extensions.Logging;
using BibleNotify.Pages;
using BibleNotify.Services;
using BibleNotify.ViewModels;
using BibleNotify.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace BibleNotify;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Localizer.ConfigureFromDeviceRegion();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if ANDROID
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<BibleNotify.Controls.ModernEntry, BibleNotify.Platforms.Android.ModernEntryHandler>();
		});
#endif

		builder.Services.AddSingleton<IBibleLibrary, BibleLibrary>();
		builder.Services.AddSingleton<IUserDataService, UserDataService>();
		builder.Services.AddSingleton<IAppNavigator, ShellNavigator>();
#if ANDROID
		builder.Services.AddSingleton<IReminderNotifier, AndroidReminderNotifier>();
#else
		builder.Services.AddSingleton<IReminderNotifier, UnavailableReminderNotifier>();
#endif
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<BibleViewModel>();
		builder.Services.AddTransient<ReadingPlanViewModel>();
		builder.Services.AddTransient<NotesViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<BiblePage>();
		builder.Services.AddTransient<ReadingPlanPage>();
		builder.Services.AddTransient<NotesPage>();
		builder.Services.AddTransient<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
