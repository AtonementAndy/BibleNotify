using Android.App;
using Android.Content;

namespace BibleNotify;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter([
    Intent.ActionBootCompleted,
    Intent.ActionTimeChanged,
    Intent.ActionTimezoneChanged,
    "android.app.action.SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED"])]
public sealed class ReminderBootReceiver : BroadcastReceiver
{
    private const string ExactAlarmPermissionChangedAction =
        "android.app.action.SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null ||
            intent?.Action is not (Intent.ActionBootCompleted or Intent.ActionTimeChanged or
                Intent.ActionTimezoneChanged or ExactAlarmPermissionChangedAction))
        {
            return;
        }

        foreach (var reminder in ReminderAlarmScheduler.Load(context))
        {
            ReminderAlarmScheduler.Schedule(context, reminder);
        }
    }
}
