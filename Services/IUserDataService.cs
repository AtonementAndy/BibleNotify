using BibleNotify.Data.Models;

namespace BibleNotify.Services;

public interface IUserDataService
{
    event EventHandler? SessionChanged;

    UserProfile? CurrentUser { get; }

    bool IsGuestSession { get; }

    Task RegisterAsync(AccountRegistration registration, CancellationToken cancellationToken = default);

    Task<bool> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    Task ContinueAsGuestAsync(CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    Task SetPremiumPreviewAsync(bool enabled, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(
        string firstName,
        string lastName,
        string ministryRole,
        string phone,
        CancellationToken cancellationToken = default);

    Task SetThemeAsync(string theme, CancellationToken cancellationToken = default);


    Task RecordBibleReadAsync(CancellationToken cancellationToken = default);

    Task<int> GetCurrentStreakAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadingPlan>> GetReadingPlansAsync(CancellationToken cancellationToken = default);

    Task StartReadingPlanAsync(int planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanDay>> GetCurrentPlanDaysAsync(CancellationToken cancellationToken = default);

    Task CompletePlanDayAsync(int dayNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BibleNote>> GetNotesAsync(CancellationToken cancellationToken = default);

    Task SaveNoteAsync(int bookId, int chapter, int verse, string text, CancellationToken cancellationToken = default);

    Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrayerReminder>> GetRemindersAsync(CancellationToken cancellationToken = default);

    Task UpdateReminderTimeAsync(
        int reminderId,
        TimeSpan time,
        CancellationToken cancellationToken = default);

    Task<int> CreateReminderAsync(
        string title,
        string message,
        TimeSpan time,
        CancellationToken cancellationToken = default);

    Task UpdateReminderAsync(
        int reminderId,
        string title,
        string message,
        TimeSpan time,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task DeleteReminderAsync(int reminderId, CancellationToken cancellationToken = default);
}
