namespace BibleNotify.Controls;

public sealed class ModernEditor : Editor
{
    public ModernEditor()
    {
        HandlerChanged += (_, _) => RemoveNativeBackground();
        Loaded += (_, _) => RemoveNativeBackground();
    }

    private void RemoveNativeBackground()
    {
#if ANDROID
        if (Handler?.PlatformView is Android.Widget.EditText platformView)
        {
            platformView.Background = new Android.Graphics.Drawables.ColorDrawable(
                Android.Graphics.Color.Transparent);
            platformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(
                Android.Graphics.Color.Transparent);
            var displayMetrics = platformView.Resources?.DisplayMetrics;
            if (displayMetrics is not null)
            {
                var padding = (int)(12 * displayMetrics.Density + 0.5f);
                platformView.SetPadding(padding, padding, padding, padding);
            }
            platformView.InputType =
                Android.Text.InputTypes.ClassText |
                Android.Text.InputTypes.TextFlagMultiLine |
                Android.Text.InputTypes.TextFlagCapSentences |
                Android.Text.InputTypes.TextFlagAutoCorrect;
            var inputLocale = Java.Util.Locale.ForLanguageTag(
                System.Globalization.CultureInfo.CurrentUICulture.Name);
            platformView.TextLocale = inputLocale;
            platformView.ImeHintLocales = Android.OS.LocaleList.ForLanguageTags(
                System.Globalization.CultureInfo.CurrentUICulture.Name);
        }
#endif
    }
}
