using BibleNotify.Pages;
using BibleNotify.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BibleNotify;

public partial class AppShell : Shell
{
	private readonly IBibleLibrary _bibleLibrary;
	private readonly IUserDataService _userData;
	private readonly ILogger<AppShell> _logger;
	private bool _startupHandled;

	public AppShell(
		IServiceProvider services,
		IBibleLibrary bibleLibrary,
		IUserDataService userData,
		ILogger<AppShell> logger)
	{
		_bibleLibrary = bibleLibrary;
		_userData = userData;
		_logger = logger;
		InitializeComponent();

		LoginContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<LoginPage>());
		HomeContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<HomePage>());
		ReadingPlanContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<ReadingPlanPage>());
		BibleContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<BiblePage>());
		NotesContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<NotesPage>());
		SettingsContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<SettingsPage>());
		Loaded += OnLoaded;
	}

	public Task NavigateToDashboardAsync() => GoToAsync("//main/home");

	private async void OnLoaded(object? sender, EventArgs args)
	{
		if (_startupHandled)
		{
			return;
		}

		_startupHandled = true;
		try
		{
			if (!await _userData.RestoreSessionAsync())
			{
				return;
			}

			await _bibleLibrary.InitializeAsync();
			await GoToAsync("//main/home");
		}
		catch (InvalidDataException exception)
		{
			_logger.LogError(exception, "Could not restore the saved app session.");
		}
		catch (IOException exception)
		{
			_logger.LogError(exception, "Could not initialize app data for the saved session.");
		}
		catch (SqliteException exception)
		{
			_logger.LogError(exception, "Could not read the saved local account.");
		}
	}
}
