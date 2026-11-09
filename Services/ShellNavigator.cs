namespace BibleNotify.Services;

public sealed class ShellNavigator : IAppNavigator
{
    public Task NavigateToDashboardAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//main/home"));

    public Task NavigateToBibleAsync(int? bookId = null, int? chapter = null) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            var parameters = new Dictionary<string, object>();
            if (bookId.HasValue)
            {
                parameters["bookId"] = bookId.Value;
            }

            if (chapter.HasValue)
            {
                parameters["chapter"] = chapter.Value;
            }

            return Shell.Current.GoToAsync("//main/bible", parameters);
        });

    public Task NavigateToNotesAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//main/notes"));

    public Task NavigateToLoginAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//login"));

    public Task NavigateToRegistrationAsync() =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync(
                "//login",
                new Dictionary<string, object> { ["register"] = true }));

    public Task NavigateToPremiumAsync() =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync(
                "//main/settings",
                new Dictionary<string, object> { ["section"] = "premium" }));
}
