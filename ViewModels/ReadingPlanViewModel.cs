using System.Collections.ObjectModel;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class ReadingPlanViewModel(
    IUserDataService userData,
    IAppNavigator navigator,
    ILogger<ReadingPlanViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<ReadingPlan> Plans { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<PlanDay> Days { get; set; } = [];

    [ObservableProperty]
    public partial ReadingPlan? SelectedPlan { get; set; }

    [ObservableProperty]
    public partial bool IsGuest { get; set; }

    public bool IsAccount => !IsGuest;

    public bool IsFreeAccount => !IsGuest && !IsPremium;

    public bool IsPremiumLocked => !IsPremium;

    [ObservableProperty]
    public partial bool IsPremium { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? InfoMessage { get; set; }

    public bool HasPlan => Days.Count > 0;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string SelectedPlanActionText => !IsPremium
        ? Localizer.Get("🔒 Disponível no Premium")
        : SelectedPlan?.IsActive == true
            ? Localizer.Get("Continuar plano")
            : Localizer.Get("Iniciar plano");

    public string UpgradeButtonText => IsGuest
        ? Localizer.Get("Criar conta e conhecer o Premium")
        : Localizer.Get("Ver prévia Premium");

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsGuest = userData.CurrentUser is null;
        IsPremium = userData.CurrentUser?.IsPremiumPreview == true;
        OnPropertyChanged(nameof(IsAccount));
        OnPropertyChanged(nameof(IsFreeAccount));
        OnPropertyChanged(nameof(IsPremiumLocked));
        OnPropertyChanged(nameof(UpgradeButtonText));

        IsBusy = true;
        ErrorMessage = null;
        InfoMessage = null;
        try
        {
            SetPlans(await userData.GetReadingPlansAsync());
            Days = IsPremium
                ? new ObservableCollection<PlanDay>(await userData.GetCurrentPlanDaysAsync())
                : [];
            OnPropertyChanged(nameof(HasPlan));
        }
        catch (UnauthorizedAccessException)
        {
            IsGuest = true;
            OnPropertyChanged(nameof(IsAccount));
            OnPropertyChanged(nameof(IsFreeAccount));
            OnPropertyChanged(nameof(IsPremiumLocked));
            OnPropertyChanged(nameof(UpgradeButtonText));
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not load reading plans.");
            ErrorMessage = Localizer.Get("Não foi possível carregar os planos. Tente novamente.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectPlan(ReadingPlan plan)
    {
        SetPlans(Plans, plan.PlanId);
    }

    [RelayCommand]
    private async Task StartPlanAsync()
    {
        if (SelectedPlan is null)
        {
            return;
        }

        if (!IsPremium)
        {
            await UpgradeAsync();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        InfoMessage = null;
        try
        {
            await userData.StartReadingPlanAsync(SelectedPlan.PlanId);
            await LoadCoreAsync();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            logger.LogError(exception, "The selected reading plan could not be created.");
            ErrorMessage = Localizer.Get("Este plano ainda não está disponível.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not start the selected reading plan.");
            ErrorMessage = Localizer.Get("Não foi possível iniciar o plano. Tente novamente.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDayAsync(PlanDay day)
    {
        await navigator.NavigateToBibleAsync(day.BookId, day.Chapter);
    }

    [RelayCommand]
    private async Task CompleteDayAsync(PlanDay day)
    {
        if (!IsPremium)
        {
            InfoMessage = Localizer.Get("Marcar o progresso do plano é um benefício Premium.");
            return;
        }

        try
        {
            await userData.CompletePlanDayAsync(day.DayNumber);
            Days = new ObservableCollection<PlanDay>(await userData.GetCurrentPlanDaysAsync());
            SetPlans(await userData.GetReadingPlansAsync());
            OnPropertyChanged(nameof(HasPlan));
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not update reading plan progress.");
            ErrorMessage = Localizer.Get("Não foi possível atualizar o progresso. Tente novamente.");
        }
    }

    [RelayCommand]
    private Task SignInAsync() => navigator.NavigateToLoginAsync();

    [RelayCommand]
    private Task OpenSettingsAsync() => MainThread.InvokeOnMainThreadAsync(
        () => Shell.Current.GoToAsync("//main/settings"));

    [RelayCommand]
    private Task UpgradeAsync() => navigator.NavigateToPremiumAsync();

    private async Task LoadCoreAsync()
    {
        IsGuest = userData.CurrentUser is null;
        IsPremium = userData.CurrentUser?.IsPremiumPreview == true;
        OnPropertyChanged(nameof(IsAccount));
        OnPropertyChanged(nameof(IsFreeAccount));
        OnPropertyChanged(nameof(IsPremiumLocked));
        OnPropertyChanged(nameof(UpgradeButtonText));
        SetPlans(await userData.GetReadingPlansAsync());
        Days = new ObservableCollection<PlanDay>(await userData.GetCurrentPlanDaysAsync());
        OnPropertyChanged(nameof(HasPlan));
    }

    private void SetPlans(IEnumerable<ReadingPlan> plans, int? selectedPlanId = null)
    {
        var planList = plans.ToList();
        var selectedId = selectedPlanId
            ?? planList.FirstOrDefault(plan => plan.IsActive)?.PlanId
            ?? planList.FirstOrDefault()?.PlanId;

        Plans = new ObservableCollection<ReadingPlan>(
            planList.Select(plan => plan with
            {
                Title = Localizer.GetPlanTitle(plan.PlanId, plan.Title),
                Description = Localizer.GetPlanDescription(plan.PlanId, plan.Description),
                IsSelected = plan.PlanId == selectedId
            }));
        SelectedPlan = Plans.FirstOrDefault(plan => plan.IsSelected);
    }

    partial void OnIsPremiumChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFreeAccount));
        OnPropertyChanged(nameof(IsPremiumLocked));
        OnPropertyChanged(nameof(SelectedPlanActionText));
    }

    partial void OnSelectedPlanChanged(ReadingPlan? value)
    {
        OnPropertyChanged(nameof(SelectedPlanActionText));
    }

    partial void OnDaysChanged(ObservableCollection<PlanDay> value) =>
        OnPropertyChanged(nameof(HasPlan));

    partial void OnErrorMessageChanged(string? value) =>
        OnPropertyChanged(nameof(HasError));
}
