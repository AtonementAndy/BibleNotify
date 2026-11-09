using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using BibleNotify.Localization;

namespace BibleNotify;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderReceiver : BroadcastReceiver
{
    public const string ReminderIdKey = "reminder-id";
    public const string TitleKey = "reminder-title";
    public const string MessageKey = "reminder-message";
    public const string IsDefaultKey = "reminder-is-default";
    private const string ChannelId = "bible-reading";

    public override void OnReceive(Context? context, Intent? intent)
    {
        Localizer.ConfigureFromDeviceLanguage();
        if (context is null || intent is null)
        {
            return;
        }

        var receiverContext = context!;
        var receiverIntent = intent!;
        var reminder = ReminderAlarmScheduler.Load(receiverContext)
            .FirstOrDefault(item =>
                item.ReminderId == receiverIntent.GetIntExtra(ReminderIdKey, 0));
        if (reminder is not null)
        {
            ReminderAlarmScheduler.Schedule(receiverContext, reminder);
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
            ContextCompat.CheckSelfPermission(receiverContext, global::Android.Manifest.Permission.PostNotifications) !=
            global::Android.Content.PM.Permission.Granted)
        {
            return;
        }

        var notificationManager = NotificationManagerCompat.From(receiverContext);
        if (notificationManager is null)
        {
            return;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var platformManager = (NotificationManager?)receiverContext.GetSystemService(Context.NotificationService);
            platformManager?.CreateNotificationChannel(new NotificationChannel(
                ChannelId,
                Localizer.Get("Lembretes de leitura"),
                NotificationImportance.Default));
        }

        var notificationBuilder = new NotificationCompat.Builder(receiverContext, ChannelId);
        if (notificationBuilder is null)
        {
            return;
        }

        notificationBuilder.SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo);
        var isDefaultReminder = receiverIntent.GetBooleanExtra(IsDefaultKey, false);
        var reminderTitle = receiverIntent.GetStringExtra(TitleKey) ?? "BibleNotify";
        var reminderMessage = receiverIntent.GetStringExtra(MessageKey)
            ?? Localizer.Get("Um momento para a Palavra.");
        var notificationContent = Localizer.GetReminderNotification(
            isDefaultReminder,
            reminderTitle,
            reminderMessage,
            DateTime.Now);
        notificationBuilder.SetContentTitle(notificationContent.Title);
        notificationBuilder.SetContentText(notificationContent.Message);
        notificationBuilder.SetAutoCancel(true);
        notificationBuilder.SetPriority((int)NotificationPriority.Default);
        var launchIntent = new Intent(receiverContext, typeof(MainActivity))
            .SetAction(Intent.ActionMain)
            .AddCategory(Intent.CategoryLauncher)
            .SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        var contentIntent = PendingIntent.GetActivity(
            receiverContext,
            receiverIntent.GetIntExtra(ReminderIdKey, 0),
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        notificationBuilder.SetContentIntent(contentIntent);
        var notification = notificationBuilder.Build();
        if (notification is null)
        {
            return;
        }

        notificationManager.Notify(receiverIntent.GetIntExtra(ReminderIdKey, 0), notification);
    }
}
