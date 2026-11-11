using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

namespace BibleNotify;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    LaunchMode = LaunchMode.SingleTop, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnNewIntent(Android.Content.Intent? intent)
    {
        base.OnNewIntent(intent);
        if (intent?.GetBooleanExtra(ReminderReceiver.OpenDashboardKey, false) != true)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page is AppShell shell)
            {
                await shell.NavigateToDashboardAsync();
            }
        });
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if (OperatingSystem.IsAndroidVersionAtLeast(35))
        {
            return;
        }

        Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#242C3B"));
        Window?.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#222834"));
    }
}
