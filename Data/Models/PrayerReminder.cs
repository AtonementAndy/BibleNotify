namespace BibleNotify.Data.Models;

public sealed record PrayerReminder(
    int ReminderId,
    string Title,
    string Message,
    int Hour,
    int Minute,
    bool IsEnabled,
    bool IsDefault)
{
    public TimeSpan Time => new(Hour, Minute, 0);

    public string TimeLabel => Time.ToString(@"hh\:mm");
}
