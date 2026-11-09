using System.Text.Json;
using Android.App;
using Android.Content;

namespace BibleNotify;

internal sealed record ScheduledReminder(
    int RequestCode,
    int ReminderId,
    string Title,
    string Message,
    int Hour,
    int Minute,
    bool IsDefault);

internal static class ReminderAlarmScheduler
{
    private const string PreferencesName = "bible-notify-reminders";
    private const string ScheduledRemindersKey = "scheduled-reminders";

    public static void Schedule(
        Context context,
        ScheduledReminder reminder,
        bool allowCurrentMinute = false)
    {
        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager
            ?? throw new InvalidOperationException("Android alarm service is unavailable.");
        var intent = CreateIntent(context, reminder);
        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            reminder.RequestCode,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)
            ?? throw new InvalidOperationException("Could not create an Android reminder intent.");

        var now = DateTime.Now;
        var nextOccurrence = now.Date.AddHours(reminder.Hour).AddMinutes(reminder.Minute);
        if (nextOccurrence <= now &&
            (!allowCurrentMinute || nextOccurrence.AddMinutes(1) <= now))
        {
            nextOccurrence = nextOccurrence.AddDays(1);
        }

        var triggerAtMillis = new DateTimeOffset(nextOccurrence).ToUnixTimeMilliseconds();
        if (!OperatingSystem.IsAndroidVersionAtLeast(31) || alarmManager.CanScheduleExactAlarms())
        {
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        }
        else
        {
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        }
    }

    public static void Cancel(Context context, int requestCode)
    {
        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager
            ?? throw new InvalidOperationException("Android alarm service is unavailable.");
        var intent = new Intent(context, typeof(ReminderReceiver));
        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            requestCode,
            intent,
            PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
        if (pendingIntent is not null)
        {
            alarmManager.Cancel(pendingIntent);
            pendingIntent.Cancel();
        }
    }

    public static IReadOnlyList<ScheduledReminder> Load(Context context)
    {
        var json = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private)
            ?.GetString(ScheduledRemindersKey, null);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<ScheduledReminder>>(json) ?? [];
    }

    public static void Save(Context context, IReadOnlyList<ScheduledReminder> reminders)
    {
        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private)
            ?? throw new InvalidOperationException("Could not open Android reminder preferences.");
        var json = JsonSerializer.Serialize(reminders);
        if (!preferences.Edit()!.PutString(ScheduledRemindersKey, json)!.Commit())
        {
            throw new InvalidOperationException("Could not persist Android reminder schedules.");
        }
    }

    private static Intent CreateIntent(Context context, ScheduledReminder reminder) =>
        new Intent(context, typeof(ReminderReceiver))
            .PutExtra(ReminderReceiver.ReminderIdKey, reminder.ReminderId)
            .PutExtra(ReminderReceiver.TitleKey, reminder.Title)
            .PutExtra(ReminderReceiver.MessageKey, reminder.Message)
            .PutExtra(ReminderReceiver.IsDefaultKey, reminder.IsDefault);
}
