using Android.App;
using Android.Content;
using Android.OS;
using BibleNotify.Localization;
using BibleNotify.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace BibleNotify;

public sealed class AndroidReminderNotifier(IUserDataService userData) : IReminderNotifier
{
    private const int RequestCodeBase = 4100;

    public async Task<bool> RequestPermissionAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                return false;
            }
        }

        var context = Android.App.Application.Context;
        if (OperatingSystem.IsAndroidVersionAtLeast(31) &&
            context.GetSystemService(Context.AlarmService) is AlarmManager alarmManager &&
            !alarmManager.CanScheduleExactAlarms())
        {
            var intent = new Intent(global::Android.Provider.Settings.ActionRequestScheduleExactAlarm)
                .SetData(Android.Net.Uri.Parse($"package:{context.PackageName}"))
                .SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }

        return true;
    }

    public Task ScheduleAsync() => ScheduleCoreAsync(scheduleIfAlreadyEnabled: false);

    public Task RefreshAsync() => ScheduleCoreAsync(scheduleIfAlreadyEnabled: true);

    private async Task ScheduleCoreAsync(bool scheduleIfAlreadyEnabled)
    {
        var context = Android.App.Application.Context;
        var userId = userData.CurrentUser?.UserId
            ?? throw new InvalidOperationException("A signed-in account is required to schedule reminders.");
        var preferenceKey = $"scheduled-reminder-ids-{userId}";
        var notificationsEnabledKey = $"reminder-notifications-enabled-{userId}";
        var previousIds = ParseRequestIds(Preferences.Default.Get(preferenceKey, string.Empty));
        var notificationsEnabled = Preferences.Default.Get(notificationsEnabledKey, previousIds.Length > 0);
        if (scheduleIfAlreadyEnabled && !notificationsEnabled)
        {
            return;
        }

        var reminders = await userData.GetRemindersAsync();
        if (!userData.HasPremiumAccess)
        {
            reminders = reminders.Where(reminder => reminder.IsDefault).ToArray();
        }

        var activeIds = new List<int>();
        var scheduledReminders = new List<ScheduledReminder>();

        foreach (var reminder in reminders)
        {
            var requestCode = RequestCodeBase + (userId * 1000) + reminder.ReminderId;
            if (!reminder.IsEnabled)
            {
                continue;
            }

            activeIds.Add(requestCode);
            var scheduledReminder = new ScheduledReminder(
                requestCode,
                reminder.ReminderId,
                reminder.IsDefault ? Localizer.Get(reminder.Title) : reminder.Title,
                reminder.IsDefault ? Localizer.Get(reminder.Message) : reminder.Message,
                reminder.Hour,
                reminder.Minute,
                reminder.IsDefault);
            scheduledReminders.Add(scheduledReminder);
        }

        var staleIds = previousIds
            .Concat(ReminderAlarmScheduler.Load(context).Select(reminder => reminder.RequestCode))
            .Except(activeIds)
            .Distinct();
        foreach (var staleId in staleIds)
        {
            ReminderAlarmScheduler.Cancel(context, staleId);
        }

        Preferences.Default.Set(preferenceKey, string.Join(',', activeIds));
        Preferences.Default.Set(notificationsEnabledKey, true);
        ReminderAlarmScheduler.Save(context, scheduledReminders);
        foreach (var reminder in scheduledReminders)
        {
            ReminderAlarmScheduler.Schedule(context, reminder, allowCurrentMinute: true);
        }
    }

    public Task CancelAsync()
    {
        var userId = userData.CurrentUser?.UserId;
        if (userId is null)
        {
            return Task.CompletedTask;
        }

        var preferenceKey = $"scheduled-reminder-ids-{userId.Value}";
        var requestIds = ParseRequestIds(Preferences.Default.Get(preferenceKey, string.Empty));
        var context = Android.App.Application.Context;
        var scheduledReminders = ReminderAlarmScheduler.Load(context);
        foreach (var requestId in requestIds.Concat(scheduledReminders.Select(reminder => reminder.RequestCode)).Distinct())
        {
            ReminderAlarmScheduler.Cancel(context, requestId);
        }

        Preferences.Default.Remove(preferenceKey);
        Preferences.Default.Remove($"reminder-notifications-enabled-{userId.Value}");
        ReminderAlarmScheduler.Save(context, []);
        return Task.CompletedTask;
    }

    private static int[] ParseRequestIds(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var requestId) ? (int?)requestId : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();
}
