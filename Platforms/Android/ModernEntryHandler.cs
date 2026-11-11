using BibleNotify.Controls;
using Microsoft.Maui.Handlers;
using AndroidEditText = global::Android.Widget.EditText;
using AndroidGravity = global::Android.Views.GravityFlags;

namespace BibleNotify.Platforms.Android;

public sealed class ModernEntryHandler : EntryHandler
{
    public static readonly IPropertyMapper<IEntry, IEntryHandler> ModernMapper =
        new PropertyMapper<IEntry, IEntryHandler>(EntryHandler.Mapper)
        {
            [nameof(IView.Background)] = MapModernBackground
        };

    public ModernEntryHandler() : base(ModernMapper)
    {
    }

    private static void MapModernBackground(IEntryHandler handler, IEntry entry)
    {
        if (handler.PlatformView is not AndroidEditText platformView)
        {
            return;
        }

        platformView.SetBackgroundResource(0);
        platformView.Background = null;
        platformView.BackgroundTintList = null;
        var displayMetrics = platformView.Resources?.DisplayMetrics;
        if (displayMetrics is not null)
        {
            var horizontalPadding = (int)(12 * displayMetrics.Density + 0.5f);
            platformView.SetPadding(horizontalPadding, 0, horizontalPadding, 0);
        }
        platformView.Gravity = AndroidGravity.CenterVertical;
    }
}
