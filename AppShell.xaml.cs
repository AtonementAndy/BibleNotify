using BibleNotify.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BibleNotify;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider services)
	{
		InitializeComponent();

		LoginContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<LoginPage>());
		HomeContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<HomePage>());
		ReadingPlanContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<ReadingPlanPage>());
		BibleContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<BiblePage>());
		NotesContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<NotesPage>());
		SettingsContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<SettingsPage>());
	}
}
