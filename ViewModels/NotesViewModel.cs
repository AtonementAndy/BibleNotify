using System.Collections.ObjectModel;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class NotesViewModel(
    IBibleLibrary bibleLibrary,
    IUserDataService userData,
    IAppNavigator navigator,
    ILogger<NotesViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<BookInfo> Books { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<BibleNote> Notes { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<BibleNoteGroup> NoteGroups { get; set; } = [];

    [ObservableProperty]
    public partial BookInfo? SelectedBook { get; set; }

    [ObservableProperty]
    public partial int SelectedChapter { get; set; } = 1;

    [ObservableProperty]
    public partial int SelectedVerse { get; set; } = 1;

    [ObservableProperty]
    public partial string NoteText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial BibleNote? EditingNote { get; set; }

    [ObservableProperty]
    public partial bool IsGuest { get; set; }

    [ObservableProperty]
    public partial bool IsPremium { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool IsLocked => IsGuest || !IsPremium;

    public double FormOpacity => IsLocked ? 0.45 : 1;

    public bool IsAccount => !IsGuest;

    public bool HasNotes => Notes.Count > 0;

    public string AccountGateButtonText => Localizer.Get(IsGuest ? "Criar conta" : "Seja Premium");

    public string AccessMessage => IsGuest
        ? Localizer.Get("Este recurso é exclusivo do Premium. Veja os planos e crie uma conta para continuar.")
        : Localizer.Get("Este recurso é exclusivo do Premium. Conheça os planos para criar e organizar suas anotações.");

    public string EmptyNotesMessage => IsLocked
        ? Localizer.Get("Suas anotações aparecerão aqui quando o Premium estiver ativo.")
        : Localizer.Get("Suas anotações aparecerão aqui.");

    public string SaveButtonText => IsLocked
        ? Localizer.Get("🔒 Salvar anotação · Premium")
        : IsEditingNote
            ? Localizer.Get("Salvar alterações")
            : Localizer.Get("Salvar anotação");

    public bool IsEditingNote => EditingNote is not null;

    public string NoteFormTitle => Localizer.Get(IsEditingNote ? "EDITANDO ANOTAÇÃO" : "NOVA ANOTAÇÃO");

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsGuest = userData.CurrentUser is null;
        IsPremium = userData.HasPremiumAccess;
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(FormOpacity));
        OnPropertyChanged(nameof(IsAccount));
        OnPropertyChanged(nameof(AccountGateButtonText));
        OnPropertyChanged(nameof(AccessMessage));
        OnPropertyChanged(nameof(EmptyNotesMessage));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(NoteFormTitle));
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (Books.Count == 0)
            {
                Books = new ObservableCollection<BookInfo>(await bibleLibrary.GetBooksAsync());
                SelectedBook = Books.FirstOrDefault();
            }

            Notes = IsPremium
                ? new ObservableCollection<BibleNote>(await userData.GetNotesAsync())
                : [];
            RefreshNoteGroups();
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not load Bible notes.");
            ErrorMessage = Localizer.Get("Não foi possível carregar as anotações. Tente novamente.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not initialize the local Bible database.");
            ErrorMessage = Localizer.Get("Não foi possível carregar a Bíblia offline.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsLocked)
        {
            await ContinueFromGateAsync();
            return;
        }

        if (SelectedBook is null || string.IsNullOrWhiteSpace(NoteText))
        {
            ErrorMessage = Localizer.Get("Escolha uma referência e escreva sua anotação.");
            return;
        }

        ErrorMessage = null;
        try
        {
            if (EditingNote is { } note)
            {
                await userData.UpdateNoteAsync(
                    note.NoteId,
                    SelectedBook.BookId,
                    SelectedChapter,
                    SelectedVerse,
                    NoteText);
            }
            else
            {
                await userData.SaveNoteAsync(
                    SelectedBook.BookId,
                    SelectedChapter,
                    SelectedVerse,
                    NoteText);
            }

            Notes = new ObservableCollection<BibleNote>(await userData.GetNotesAsync());
            RefreshNoteGroups();
            NoteText = string.Empty;
            EditingNote = null;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "Could not save Bible note {NoteId}.", EditingNote?.NoteId);
            IsPremium = userData.HasPremiumAccess;
            ErrorMessage = exception.Message;
        }
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not save a Bible note.");
            ErrorMessage = Localizer.Get("Não foi possível salvar a anotação.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not refresh the Bible notes after saving.");
            ErrorMessage = Localizer.Get("Anotação salva, mas não foi possível atualizar a lista.");
        }
    }

    [RelayCommand]
    private void Edit(BibleNote note)
    {
        if (IsLocked)
        {
            return;
        }

        EditingNote = note;
        SelectedBook = Books.FirstOrDefault(book => book.BookId == note.BookId);
        SelectedChapter = note.Chapter;
        SelectedVerse = note.Verse;
        NoteText = note.Text;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingNote = null;
        NoteText = string.Empty;
        SelectedChapter = 1;
        SelectedVerse = 1;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task DeleteAsync(BibleNote note)
    {
        try
        {
            await userData.DeleteNoteAsync(note.NoteId);
            Notes.Remove(note);
            if (EditingNote?.NoteId == note.NoteId)
            {
                CancelEdit();
            }

            RefreshNoteGroups();
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not delete Bible note {NoteId}.", note.NoteId);
            ErrorMessage = Localizer.Get("Não foi possível excluir a anotação.");
        }
    }

    [RelayCommand]
    private Task ContinueFromGateAsync() => IsGuest
        ? navigator.NavigateToRegistrationAsync()
        : navigator.NavigateToPremiumAsync();

    partial void OnNotesChanged(ObservableCollection<BibleNote> value) =>
        OnPropertyChanged(nameof(HasNotes));

    private void RefreshNoteGroups()
    {
        NoteGroups = new ObservableCollection<BibleNoteGroup>(
            Notes.GroupBy(note => new { note.BookId, note.BookName })
                .OrderBy(group => group.Key.BookId)
                .Select(group =>
                {
                    var noteGroup = new BibleNoteGroup(group.Key.BookId, group.Key.BookName);
                    foreach (var note in group.OrderByDescending(note => note.UpdatedAt))
                    {
                        noteGroup.Add(note);
                    }

                    return noteGroup;
                }));
    }

    partial void OnIsPremiumChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(FormOpacity));
        OnPropertyChanged(nameof(EmptyNotesMessage));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    partial void OnIsGuestChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(FormOpacity));
        OnPropertyChanged(nameof(IsAccount));
        OnPropertyChanged(nameof(AccountGateButtonText));
        OnPropertyChanged(nameof(AccessMessage));
        OnPropertyChanged(nameof(EmptyNotesMessage));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    partial void OnEditingNoteChanged(BibleNote? value)
    {
        OnPropertyChanged(nameof(IsEditingNote));
        OnPropertyChanged(nameof(NoteFormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }
}
