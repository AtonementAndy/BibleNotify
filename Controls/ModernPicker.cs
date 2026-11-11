namespace BibleNotify.Controls;

public sealed class ModernPicker : Picker
{
    public ModernPicker()
    {
        HandlerChanged += (_, _) => RemoveNativeBackground();
        Loaded += (_, _) => RemoveNativeBackground();
    }

    private void RemoveNativeBackground()
    {
#if ANDROID
        if (Handler?.PlatformView is Android.Widget.EditText platformView)
        {
            platformView.Background = null;
            platformView.BackgroundTintList = null;
            platformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            var displayMetrics = platformView.Resources?.DisplayMetrics;
            if (displayMetrics is not null)
            {
                var horizontalPadding = (int)(8 * displayMetrics.Density + 0.5f);
                platformView.SetPadding(horizontalPadding, 0, horizontalPadding, 0);
            }
            platformView.Gravity = Android.Views.GravityFlags.CenterVertical;
        }
#endif
    }
}
