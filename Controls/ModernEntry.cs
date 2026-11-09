namespace BibleNotify.Controls;

public sealed class ModernEntry : Entry
{
    public ModernEntry()
    {
        HandlerChanged += (_, _) => RemoveNativeBackground();
        Loaded += (_, _) => RemoveNativeBackground();
    }

    private void RemoveNativeBackground()
    {
#if IOS || MACCATALYST
        if (Handler?.PlatformView is UIKit.UITextField platformView)
        {
            platformView.BorderStyle = UIKit.UITextBorderStyle.None;
            platformView.BackgroundColor = UIKit.UIColor.Clear;
        }
#elif WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox platformView)
        {
            platformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            platformView.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Transparent);
            platformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Transparent);
        }
#endif
    }
}
