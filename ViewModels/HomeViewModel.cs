using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class HomeViewModel(
    IBibleLibrary bibleLibrary,
    IUserDataService userData,
    IAppNavigator navigator,
    ILogger<HomeViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial BibleVerse? DailyVerse { get; set; }

    [ObservableProperty]
    public partial string VerseReference { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string Greeting { get; set; } = Localizer.Get("Que bom ter você por aqui!");

    [ObservableProperty]
    public partial string AccountLabel { get; set; } = Localizer.Get("Visitante");

    [ObservableProperty]
    public partial int ReadingStreak { get; set; }

    public string ReadingStreakLabel => Localizer.Format("{0} dias consecutivos", ReadingStreak);

    [ObservableProperty]
    public partial bool ShowPremiumStreak { get; set; }

    [ObservableProperty]
    public partial bool ShowCreateAccount { get; set; }

    [ObservableProperty]
    public partial bool HasActiveReadingPlan { get; set; }

    [ObservableProperty]
    public partial string ActivePlanTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActivePlanProgress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActivePlanNextReading { get; set; } = string.Empty;

    public bool ShowPremiumPlanCard => ShowPremiumStreak;

    public bool ShowActivePlanDetails => HasActiveReadingPlan;

    public string PremiumPlanActionText => Localizer.Get(HasActiveReadingPlan ? "Ver plano" : "Escolher plano");

    public bool IsStreakLocked => !ShowPremiumStreak;

    public string StreakUpgradeMessage => userData.IsGuestSession
        ? Localizer.Get("Acompanhe seus dias consecutivos com uma conta Premium.")
        : Localizer.Get("Acompanhe seus dias consecutivos com o Premium.");

    [RelayCommand]
    private async Task LoadAsync()
    {
        UpdateGreeting();
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            if (DailyVerse is null)
            {
                var previousVerseId = Preferences.Get("dashboard-last-verse-id", 0);
                DailyVerse = await bibleLibrary.GetRandomVerseAsync(
                    previousVerseId > 0 ? previousVerseId : null);
                if (DailyVerse is not null)
                {
                    Preferences.Set("dashboard-last-verse-id", DailyVerse.VerseId);
                    var books = await bibleLibrary.GetBooksAsync();
                    var bookName = books.FirstOrDefault(book => book.BookId == DailyVerse.BookId)?.Name;
                    VerseReference = bookName is null
                        ? Localizer.Format("{0} · Bíblia Livre", $"{DailyVerse.Chapter}:{DailyVerse.Verse}")
                        : Localizer.Format("{0} {1}:{2} · Bíblia Livre", bookName, DailyVerse.Chapter, DailyVerse.Verse);
                }
            }

            if (DailyVerse is null)
            {
                ErrorMessage = Localizer.Get("Não foi possível escolher um versículo para hoje.");
            }

            if (userData.CurrentUser?.IsPremiumPreview == true)
            {
                ReadingStreak = await userData.GetCurrentStreakAsync();
                var activePlan = (await userData.GetReadingPlansAsync())
                    .FirstOrDefault(plan => plan.IsActive);
                HasActiveReadingPlan = activePlan is not null;
                ActivePlanTitle = activePlan is null
                    ? Localizer.Get("Nenhum plano iniciado")
                    : Localizer.GetPlanTitle(activePlan.PlanId, activePlan.Title);
                if (activePlan is not null)
                {
                    var days = await userData.GetCurrentPlanDaysAsync();
                    var nextDay = days.FirstOrDefault(day => !day.IsCompleted);
                    ActivePlanProgress = Localizer.Format(
                        "{0} de {1} dias concluídos",
                        activePlan.CompletedDays,
                        activePlan.TotalDays);
                    ActivePlanNextReading = nextDay is null
                        ? Localizer.Get("Parabéns! Você concluiu este plano.")
                        : Localizer.Format("Próxima leitura · Dia {0}: {1}", nextDay.DayNumber, nextDay.Reference);
                }
                else
                {
                    ActivePlanProgress = Localizer.Get("Comece uma jornada de leitura no seu ritmo.");
                    ActivePlanNextReading = Localizer.Get("Escolha um plano para organizar sua leitura diária.");
                }

                OnPropertyChanged(nameof(ShowPremiumPlanCard));
                OnPropertyChanged(nameof(ShowActivePlanDetails));
                OnPropertyChanged(nameof(PremiumPlanActionText));
            }
            else
            {
                HasActiveReadingPlan = false;
                OnPropertyChanged(nameof(ShowPremiumPlanCard));
                OnPropertyChanged(nameof(ShowActivePlanDetails));
            }
        }
        catch (InvalidDataException exception)
        {
            logger.LogError(exception, "The bundled Bible database failed validation.");
            ErrorMessage = Localizer.Get("O banco da Bíblia está inválido. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not load the daily Bible verse.");
            ErrorMessage = Localizer.Get("Não foi possível carregar o versículo. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not read the dashboard data.");
            ErrorMessage = Localizer.Get("Não foi possível carregar os dados. Tente novamente.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task ReadDailyVerseAsync() => DailyVerse is null
        ? Task.CompletedTask
        : navigator.NavigateToBibleAsync(DailyVerse.BookId, DailyVerse.Chapter);

    [RelayCommand]
    private Task OpenNotesAsync() => navigator.NavigateToNotesAsync();

    [RelayCommand]
    private Task OpenReadingPlanAsync() => MainThread.InvokeOnMainThreadAsync(
        () => Shell.Current.GoToAsync("//main/plans"));

    [RelayCommand]
    private Task UpgradeAsync() => navigator.NavigateToPremiumAsync();

    [RelayCommand]
    private Task CreateAccountAsync() => navigator.NavigateToRegistrationAsync();

    private void UpdateGreeting()
    {
        var user = userData.CurrentUser;
        ShowPremiumStreak = user?.IsPremiumPreview == true;
        ShowCreateAccount = userData.IsGuestSession;
        OnPropertyChanged(nameof(IsStreakLocked));
        OnPropertyChanged(nameof(StreakUpgradeMessage));
        if (user is null)
        {
            Greeting = userData.IsGuestSession
                ? Localizer.Get("Que bom ter você por aqui!")
                : Localizer.Get("Que bom ver você novamente.");
            AccountLabel = userData.IsGuestSession ? Localizer.Get("Modo visitante") : "BibleNotify";
            return;
        }

        var role = user.MinistryRole.Trim();
        var firstName = user.FirstName.Trim();
        Greeting = Localizer.Format(
            "Olá, {0} {1}, que bom ter você por aqui.",
            Localizer.GetMinistryRole(role).ToLowerInvariant(),
            firstName);
        AccountLabel = user.MembershipName;
    }

    partial void OnReadingStreakChanged(int value) =>
        OnPropertyChanged(nameof(ReadingStreakLabel));
}
