using System.Collections.ObjectModel;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class SettingsViewModel(
    IUserDataService userData,
    IAppNavigator navigator,
    IReminderNotifier reminderNotifier,
    ILogger<SettingsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MinistryRole { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = Localizer.Get("Sistema");

    [ObservableProperty]
    public partial bool IsGuest { get; set; }

    [ObservableProperty]
    public partial bool IsPremium { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<PrayerReminder> Reminders { get; set; } = [];

    [ObservableProperty]
    public partial PrayerReminder? SelectedReminder { get; set; }

    [ObservableProperty]
    public partial TimeSpan SelectedReminderTime { get; set; } = new(8, 0, 0);

    [ObservableProperty]
    public partial bool SelectedReminderEnabled { get; set; }

    [ObservableProperty]
    public partial string ReminderTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReminderMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial TimeSpan NewReminderTime { get; set; } = new(9, 0, 0);

    [ObservableProperty]
    public partial string NewReminderTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewReminderMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? InfoMessage { get; set; }

    [ObservableProperty]
    public partial string CurrentSection { get; set; } = "menu";

    public ObservableCollection<string> MinistryRoles { get; } =
    [
        Localizer.Get("Membro"), Localizer.Get("Pastor"), Localizer.Get("Pastora"), Localizer.Get("Líder"),
        Localizer.Get("Missionário"), Localizer.Get("Missionária"), Localizer.Get("Diácono"),
        Localizer.Get("Diaconisa"), Localizer.Get("Presbítero"), Localizer.Get("Presbítera"),
        Localizer.Get("Evangelista"), Localizer.Get("Outro")
    ];

    public ObservableCollection<string> Themes { get; } =
        [Localizer.Get("Sistema"), Localizer.Get("Claro"), Localizer.Get("Escuro")];

    public bool IsAccount => !IsGuest;

    public bool IsPremiumPreview => IsPremium;

    public bool IsMenu => CurrentSection == "menu";

    public bool IsNotMenu => !IsMenu;

    public bool IsAccountSection => CurrentSection == "account";

    public bool IsPremiumSection => CurrentSection == "premium";

    public bool IsNotificationsSection => CurrentSection == "notifications";

    public bool IsThemeSection => CurrentSection == "theme";

    public string SectionTitle => CurrentSection switch
    {
        "account" => Localizer.Get("Conta"),
        "premium" => Localizer.Get("Premium"),
        "notifications" => Localizer.Get("Notificações"),
        "theme" => Localizer.Get("Tema"),
        _ => Localizer.Get("Ajustes")
    };

    public bool CanEditProfile => IsAccount;

    public bool CanChangeReminderTime => IsAccount;

    public string MonthlyPrice => Localizer.Get("R$ 9,99 / mês");

    public string SemiAnnualPrice => Localizer.Get("R$ 45,99 / 6 meses");

    public string AnnualPrice => Localizer.Get("R$ 79,99 / ano");

    public bool CanEditNotificationDetails => IsPremium;

    public bool CanManagePremiumPreferences => IsPremium;

    public bool IsNotPremium => !IsPremium;

    public bool CanEnablePremiumPreview
    {
        get
        {
#if DEBUG
            return IsAccount && !IsPremium;
#else
            return false;
#endif
        }
    }

    public bool CanDisablePremiumPreview
    {
        get
        {
#if DEBUG
            return IsAccount && IsPremium;
#else
            return false;
#endif
        }
    }

    public string PremiumGateMessage => IsGuest
        ? Localizer.Get("Recurso Premium. Entre ou crie uma conta para conhecer a prévia.")
        : Localizer.Get("Recurso exclusivo do Premium. Ative a prévia para experimentar.");

    public string AccountStatus => IsGuest
        ? Localizer.Get("Visitante · Bíblia offline")
        : IsPremium ? Localizer.Get("Premium · prévia local") : Localizer.Get("Conta gratuita");

    public string ReminderSaveButtonText => IsGuest
        ? Localizer.Get("🔒 Alterar horários · precisa de conta")
        : Localizer.Get("Salvar horário");

    public string EditingReminderTitle => SelectedReminder is null
        ? string.Empty
        : Localizer.Format("EDITANDO: {0}", SelectedReminder.Title);

    partial void OnCurrentSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsMenu));
        OnPropertyChanged(nameof(IsNotMenu));
        OnPropertyChanged(nameof(IsAccountSection));
        OnPropertyChanged(nameof(IsPremiumSection));
        OnPropertyChanged(nameof(IsNotificationsSection));
        OnPropertyChanged(nameof(IsThemeSection));
        OnPropertyChanged(nameof(SectionTitle));
    }

    public void OpenSection(string? section)
    {
        var normalizedSection = section?.Trim().ToLowerInvariant();
        CurrentSection = normalizedSection is "account" or "premium" or "notifications" or "theme"
            ? normalizedSection
            : "menu";
        if (CurrentSection == "menu")
        {
            InfoMessage = null;
            ErrorMessage = null;
        }
    }

    [RelayCommand]
    private void BackToMenu() => OpenSection("menu");

    [RelayCommand]
    private void SelectSection(string section) => OpenSection(section);

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsGuest = userData.CurrentUser is null;
        IsPremium = userData.HasPremiumAccess;
        OnPropertyChanged(nameof(IsAccount));
        OnPropertyChanged(nameof(IsPremiumPreview));
        OnPropertyChanged(nameof(IsNotPremium));
        OnPropertyChanged(nameof(CanEditProfile));
        OnPropertyChanged(nameof(CanChangeReminderTime));
        OnPropertyChanged(nameof(CanEditNotificationDetails));
        OnPropertyChanged(nameof(CanManagePremiumPreferences));
        OnPropertyChanged(nameof(CanEnablePremiumPreview));
        OnPropertyChanged(nameof(CanDisablePremiumPreview));
        OnPropertyChanged(nameof(PremiumGateMessage));
        OnPropertyChanged(nameof(AccountStatus));
        OnPropertyChanged(nameof(ReminderSaveButtonText));
        if (userData.CurrentUser is not { } user)
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            MinistryRole = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            SelectedTheme = Preferences.Get("app-theme", "System") switch
            {
                "Light" => Localizer.Get("Claro"),
                "Dark" => Localizer.Get("Escuro"),
                _ => Localizer.Get("Sistema")
            };
            Reminders =
            [
                new PrayerReminder(-1, Localizer.Get("Momento da manhã"), Localizer.Get("Comece o dia com alguns minutos na Palavra."), 8, 0, true, true),
                new PrayerReminder(-2, Localizer.Get("Pausa para refletir"), Localizer.Get("Reserve um instante para ler a Bíblia."), 18, 0, true, true)
            ];
            SelectedReminder = Reminders[0];
            return;
        }

        FirstName = user.FirstName;
        LastName = user.LastName;
        MinistryRole = Localizer.GetMinistryRole(user.MinistryRole);
        Phone = user.Phone;
        Email = user.Email;
        var savedTheme = Preferences.Get("app-theme", user.Theme);
        SelectedTheme = savedTheme switch
        {
            "Light" => Localizer.Get("Claro"),
            "Dark" => Localizer.Get("Escuro"),
            _ => Localizer.Get("Sistema")
        };
        if (Application.Current is { } app)
        {
            app.UserAppTheme = savedTheme switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };
        }

        IsBusy = true;
        try
        {
            Reminders = new ObservableCollection<PrayerReminder>(await GetAvailableRemindersAsync());
            SelectedReminder = Reminders.FirstOrDefault();
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not load user settings.");
            ErrorMessage = Localizer.Get("Não foi possível carregar as configurações.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (!IsAccount)
        {
            await SignInAsync();
            return;
        }

        try
        {
            await userData.UpdateProfileAsync(FirstName, LastName, MinistryRole, Phone);
            InfoMessage = Localizer.Get("Perfil atualizado.");
            ErrorMessage = null;
        }
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not save the user profile.");
            ErrorMessage = Localizer.Get("Não foi possível salvar o perfil.");
        }
    }

    [RelayCommand]
    private async Task TogglePremiumPreviewAsync()
    {
        if (!CanEnablePremiumPreview && !CanDisablePremiumPreview)
        {
            ErrorMessage = Localizer.Get("A prévia Premium só fica disponível em compilações de desenvolvimento.");
            return;
        }

        try
        {
            await userData.SetPremiumPreviewAsync(!IsPremium);
            IsPremium = !IsPremium;
            await reminderNotifier.RefreshAsync();
            OnPropertyChanged(nameof(IsPremiumPreview));
            OnPropertyChanged(nameof(CanEditNotificationDetails));
            OnPropertyChanged(nameof(CanManagePremiumPreferences));
            OnPropertyChanged(nameof(CanEnablePremiumPreview));
            OnPropertyChanged(nameof(CanDisablePremiumPreview));
            OnPropertyChanged(nameof(AccountStatus));
            InfoMessage = IsPremium
                ? Localizer.Get("Prévia Premium ativada neste aparelho. Nenhuma compra ou cobrança foi realizada.")
                : Localizer.Get("Prévia Premium desativada.");
            await LoadAsync();
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not change the local Premium preview.");
            ErrorMessage = Localizer.Get("Não foi possível atualizar o nível da conta.");
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "The local Premium preview is not available in this build.");
            ErrorMessage = Localizer.Get("A prévia Premium só fica disponível em compilações de desenvolvimento.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "The Premium level changed, but reminder schedules could not be refreshed.");
            ErrorMessage = Localizer.Get("O nível da conta foi atualizado, mas não foi possível atualizar os lembretes.");
        }
    }

    [RelayCommand]
    private async Task SaveThemeAsync()
    {
        var theme = SelectedTheme switch
        {
            var value when value == Localizer.Get("Claro") => "Light",
            var value when value == Localizer.Get("Escuro") => "Dark",
            _ => "System"
        };

        try
        {
            if (IsAccount)
            {
                await userData.SetThemeAsync(theme);
            }

            Preferences.Set("app-theme", theme);
            if (Application.Current is { } application)
            {
                application.UserAppTheme = theme switch
                {
                    "Light" => AppTheme.Light,
                    "System" => AppTheme.Unspecified,
                    _ => AppTheme.Dark
                };
            }
            InfoMessage = Localizer.Get("Tema atualizado.");
            ErrorMessage = null;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not save the selected app theme.");
            ErrorMessage = Localizer.Get("Não foi possível salvar o tema.");
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "Theme selection is unavailable for this account.");
            ErrorMessage = exception.Message;
        }
    }

    [RelayCommand]
    private async Task SaveReminderAsync()
    {
        if (!IsAccount)
        {
            await SignInAsync();
            return;
        }

        if (SelectedReminder is null)
        {
            ErrorMessage = Localizer.Get("Selecione um lembrete.");
            return;
        }

        try
        {
            if (IsPremium)
            {
                await userData.UpdateReminderAsync(
                    SelectedReminder.ReminderId,
                    ReminderTitle,
                    ReminderMessage,
                    SelectedReminderTime,
                    SelectedReminderEnabled);
            }
            else
            {
                await userData.UpdateReminderTimeAsync(SelectedReminder.ReminderId, SelectedReminderTime);
            }

            await RefreshRemindersAsync(SelectedReminder.ReminderId);
            await reminderNotifier.RefreshAsync();
            InfoMessage = Localizer.Get("Lembrete atualizado.");
            ErrorMessage = null;
        }
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "A reminder edit was rejected by the account access rules.");
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not save reminder settings.");
            ErrorMessage = Localizer.Get("Não foi possível salvar o lembrete.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "The reminder was saved, but its Android schedule could not be refreshed.");
            ErrorMessage = Localizer.Get("Horário salvo, mas não foi possível atualizar o agendamento no aparelho.");
        }
    }

    [RelayCommand]
    private async Task AddReminderAsync()
    {
        try
        {
            var reminderId = await userData.CreateReminderAsync(
                NewReminderTitle,
                NewReminderMessage,
                NewReminderTime);
            NewReminderTitle = string.Empty;
            NewReminderMessage = string.Empty;
            await RefreshRemindersAsync(reminderId);
            await reminderNotifier.RefreshAsync();
            InfoMessage = Localizer.Get("Novo lembrete criado.");
            ErrorMessage = null;
        }
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "A custom reminder was rejected by the account access rules.");
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not create a reminder.");
            ErrorMessage = Localizer.Get("Não foi possível criar o lembrete.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "The reminder was created, but its Android schedule could not be refreshed.");
            ErrorMessage = Localizer.Get("Lembrete criado, mas não foi possível atualizar o agendamento no aparelho.");
        }
    }

    [RelayCommand]
    private void EditReminder(PrayerReminder reminder)
    {
        SelectedReminder = Reminders.FirstOrDefault(item => item.ReminderId == reminder.ReminderId);
        OnPropertyChanged(nameof(EditingReminderTitle));
        InfoMessage = Localizer.Format("Editando: {0}", reminder.Title);
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task DeleteReminderAsync(PrayerReminder? reminder)
    {
        if (reminder is null)
        {
            return;
        }

        try
        {
            await userData.DeleteReminderAsync(reminder.ReminderId);
            await RefreshRemindersAsync();
            await reminderNotifier.RefreshAsync();
            InfoMessage = Localizer.Get("Lembrete excluído.");
            ErrorMessage = null;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "A reminder deletion was rejected by the account access rules.");
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not delete the reminder.");
            ErrorMessage = Localizer.Get("Não foi possível excluir o lembrete.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "The reminder was deleted, but its Android schedule could not be refreshed.");
            ErrorMessage = Localizer.Get("Lembrete excluído, mas não foi possível atualizar o agendamento no aparelho.");
        }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        try
        {
            await reminderNotifier.CancelAsync();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Could not cancel scheduled reminders before sign-out.");
            ErrorMessage = Localizer.Get("Não foi possível cancelar os lembretes. Tente novamente.");
            return;
        }

        await userData.SignOutAsync();
        await navigator.NavigateToLoginAsync();
    }

    [RelayCommand]
    private Task SignInAsync() => navigator.NavigateToLoginAsync();

    [RelayCommand]
    private Task CreateAccountAsync() => navigator.NavigateToRegistrationAsync();

    [RelayCommand]
    private Task UpgradeAsync() => navigator.NavigateToPremiumAsync();

    [RelayCommand]
    private async Task PurchasePlanAsync(string plan)
    {
        var message = IsGuest
            ? Localizer.Get("Entre ou crie uma conta para continuar. A integração de pagamento ainda será configurada.")
            : Localizer.Get("A integração de pagamento ainda não está configurada. Nenhuma cobrança foi realizada.");
        var planTitle = plan switch
        {
            "mensal" => Localizer.Get("Mensal"),
            "semestral" => Localizer.Get("Semestral"),
            "anual" => Localizer.Get("Anual"),
            _ => plan
        };
        await Shell.Current.DisplayAlertAsync(Localizer.Format("Plano {0}", planTitle), message, Localizer.Get("Entendi"));
    }

    [RelayCommand]
    private async Task EnableNotificationsAsync()
    {
        if (!IsAccount)
        {
            await SignInAsync();
            return;
        }

        try
        {
            if (!await reminderNotifier.RequestPermissionAsync())
            {
                ErrorMessage = Localizer.Get("Conceda a permissão de notificações e toque novamente em ativar.");
                return;
            }

            await reminderNotifier.ScheduleAsync();
            ErrorMessage = null;
            InfoMessage = Localizer.Get("Lembretes diários ativados neste aparelho.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not load reminders for notification scheduling.");
            ErrorMessage = Localizer.Get("Não foi possível carregar os lembretes.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Could not schedule local notifications.");
            ErrorMessage = Localizer.Get("Não foi possível ativar os lembretes neste aparelho.");
        }
    }

    partial void OnSelectedReminderChanged(PrayerReminder? value)
    {
        OnPropertyChanged(nameof(EditingReminderTitle));
        if (value is not null)
        {
            SelectedReminderTime = value.Time;
            SelectedReminderEnabled = value.IsEnabled;
            ReminderTitle = value.Title;
            ReminderMessage = value.Message;
        }
    }

    partial void OnIsPremiumChanged(bool value)
    {
        OnPropertyChanged(nameof(IsPremiumPreview));
        OnPropertyChanged(nameof(IsNotPremium));
        OnPropertyChanged(nameof(CanEditNotificationDetails));
        OnPropertyChanged(nameof(CanManagePremiumPreferences));
        OnPropertyChanged(nameof(CanEnablePremiumPreview));
        OnPropertyChanged(nameof(CanDisablePremiumPreview));
        OnPropertyChanged(nameof(PremiumGateMessage));
        OnPropertyChanged(nameof(AccountStatus));
        OnPropertyChanged(nameof(ReminderSaveButtonText));
    }

    private async Task RefreshRemindersAsync(int? preferredReminderId = null)
    {
        Reminders = new ObservableCollection<PrayerReminder>(await GetAvailableRemindersAsync());
        SelectedReminder = Reminders.FirstOrDefault(reminder =>
            reminder.ReminderId == (preferredReminderId ?? SelectedReminder?.ReminderId))
            ?? Reminders.FirstOrDefault();
    }

    private async Task<IReadOnlyList<PrayerReminder>> GetAvailableRemindersAsync()
    {
        var reminders = await userData.GetRemindersAsync();
        return (IsPremium ? reminders : reminders.Where(reminder => reminder.IsDefault))
            .Select(reminder => reminder.IsDefault
                ? reminder with
                {
                    Title = Localizer.Get(reminder.Title),
                    Message = Localizer.Get(reminder.Message)
                }
                : reminder)
            .ToArray();
    }
}
