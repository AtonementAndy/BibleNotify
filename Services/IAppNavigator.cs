namespace BibleNotify.Services;

public interface IAppNavigator
{
    Task NavigateToDashboardAsync();

    Task NavigateToBibleAsync(int? bookId = null, int? chapter = null);

    Task NavigateToNotesAsync();

    Task NavigateToLoginAsync();

    Task NavigateToRegistrationAsync();

    Task NavigateToPremiumAsync();
}
