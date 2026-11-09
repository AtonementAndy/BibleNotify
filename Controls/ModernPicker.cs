namespace BibleNotify.Controls;

public sealed class ModernPicker : Picker
{
    public ModernPicker()
    {
        HandlerChanged += (_, _) => Dispatcher.Dispatch(RemoveNativeBackground);
    }

    private void RemoveNativeBackground()
    {
#if ANDROID
        if (Handler?.PlatformView is Android.Widget.EditText platformView)
        {
            platformView.Background = null;
            platformView.BackgroundTintList = null;
            platformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            var horizontalPadding = (int)(8 * platformView.Resources!.DisplayMetrics!.Density + 0.5f);
            platformView.SetPadding(horizontalPadding, 0, horizontalPadding, 0);
            platformView.Gravity = Android.Views.GravityFlags.CenterVertical;
        }
#endif
    }
}
