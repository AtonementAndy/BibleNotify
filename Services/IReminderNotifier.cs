namespace BibleNotify.Services;

public interface IReminderNotifier
{
    Task<bool> RequestPermissionAsync();

    Task ScheduleAsync();

    Task RefreshAsync();

    Task CancelAsync();
}
