namespace BibleNotify.Services;

public sealed class UnavailableReminderNotifier : IReminderNotifier
{
    public Task<bool> RequestPermissionAsync() => Task.FromResult(false);

    public Task ScheduleAsync() => Task.CompletedTask;

    public Task RefreshAsync() => Task.CompletedTask;

    public Task CancelAsync() => Task.CompletedTask;
}
