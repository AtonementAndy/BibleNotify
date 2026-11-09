using System.Collections.ObjectModel;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class LoginViewModel(
    IBibleLibrary bibleLibrary,
    IUserDataService userData,
    IAppNavigator navigator,
    ILogger<LoginViewModel> logger) : ObservableObject, IQueryAttributable
{
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("register", out var register))
        {
            IsRegistering = string.Equals(
                Convert.ToString(register),
                "true",
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [NotifyCanExecuteChangedFor(nameof(ContinueAsGuestCommand))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    public partial bool IsRegistering { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedMinistryRole { get; set; } = Localizer.Get("Membro");

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public ObservableCollection<string> MinistryRoles { get; } =
    [
        Localizer.Get("Membro"),
        Localizer.Get("Pastor"),
        Localizer.Get("Pastora"),
        Localizer.Get("Líder"),
        Localizer.Get("Missionário"),
        Localizer.Get("Missionária"),
        Localizer.Get("Diácono"),
        Localizer.Get("Diaconisa"),
        Localizer.Get("Presbítero"),
        Localizer.Get("Presbítera"),
        Localizer.Get("Evangelista"),
        Localizer.Get("Outro")
    ];

    public string SubmitButtonText => Localizer.Get(IsRegistering ? "Criar minha conta" : "Entrar");

    public string ModeSwitchText => IsRegistering
        ? Localizer.Get("Já tem uma conta? Entrar")
        : Localizer.Get("Ainda não tem conta? Criar conta");

    public string PasswordPlaceholder => Localizer.Get(
        IsRegistering ? "Senha (mín. 8 caracteres)" : "Senha");

    public ReturnType PasswordReturnType => IsRegistering ? ReturnType.Next : ReturnType.Done;

    partial void OnIsRegisteringChanged(bool value)
    {
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = null;
        OnPropertyChanged(nameof(SubmitButtonText));
        OnPropertyChanged(nameof(ModeSwitchText));
        OnPropertyChanged(nameof(PasswordPlaceholder));
        OnPropertyChanged(nameof(PasswordReturnType));
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            if (IsRegistering)
            {
                if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
                {
                    ErrorMessage = Localizer.Get("As senhas não conferem.");
                    return;
                }

                await userData.RegisterAsync(new AccountRegistration(
                    FirstName,
                    LastName,
                    SelectedMinistryRole,
                    Phone,
                    Email,
                    Password));
            }
            else if (!await userData.SignInAsync(Email, Password))
            {
                ErrorMessage = Localizer.Get("E-mail ou senha incorretos.");
                return;
            }

            await bibleLibrary.InitializeAsync();
            await navigator.NavigateToDashboardAsync();
        }
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            logger.LogWarning(exception, "A local account already uses this email address.");
            ErrorMessage = Localizer.Get("Já existe uma conta com este e-mail.");
        }
        catch (InvalidDataException exception)
        {
            logger.LogError(exception, "A local database failed validation.");
            ErrorMessage = Localizer.Get("Não foi possível abrir os dados locais. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not initialize the local application data.");
            ErrorMessage = Localizer.Get("Não foi possível preparar o aplicativo. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not access local account data.");
            ErrorMessage = Localizer.Get("Não foi possível acessar a conta local. Tente novamente.");
        }
        finally
        {
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleMode() => IsRegistering = !IsRegistering;

    [RelayCommand(CanExecute = nameof(CanContinueAsGuest))]
    private async Task ContinueAsGuestAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await bibleLibrary.InitializeAsync();
            await userData.ContinueAsGuestAsync();
            await navigator.NavigateToDashboardAsync();
        }
        catch (InvalidDataException exception)
        {
            logger.LogError(exception, "The bundled Bible database failed validation.");
            ErrorMessage = Localizer.Get("O banco da Bíblia está inválido. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not load the bundled Bible database.");
            ErrorMessage = Localizer.Get("Não foi possível preparar a Bíblia offline. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not open the bundled Bible database.");
            ErrorMessage = Localizer.Get("Não foi possível abrir a Bíblia offline. Tente novamente.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSubmit() => !IsBusy;

    private bool CanContinueAsGuest() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
}
