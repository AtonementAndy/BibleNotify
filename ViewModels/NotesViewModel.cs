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
    public partial ObservableCollection<int> Chapters { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<int> Verses { get; set; } = [];

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
    public partial bool IsReferenceLoading { get; set; }

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

    private bool _suppressReferenceUpdates;
    private int _referenceLoadVersion;

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
                _suppressReferenceUpdates = true;
                SelectedBook = Books.FirstOrDefault();
                _suppressReferenceUpdates = false;
            }

            if (SelectedBook is { } selectedBook)
            {
                await LoadReferenceAsync(selectedBook, SelectedChapter, SelectedVerse, refreshChapters: true);
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

        if (IsReferenceLoading ||
            !Chapters.Contains(SelectedChapter) ||
            !Verses.Contains(SelectedVerse))
        {
            ErrorMessage = Localizer.Get("Escolha um capítulo e versículo disponíveis.");
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
    private async Task EditAsync(BibleNote note)
    {
        if (IsLocked)
        {
            return;
        }

        EditingNote = note;
        ErrorMessage = null;
        var book = Books.FirstOrDefault(item => item.BookId == note.BookId);
        if (book is null)
        {
            ErrorMessage = Localizer.Get("A referência escolhida não está disponível na Bíblia.");
            return;
        }

        _suppressReferenceUpdates = true;
        SelectedBook = book;
        _suppressReferenceUpdates = false;
        await LoadReferenceAsync(book, note.Chapter, note.Verse, refreshChapters: true);
        NoteText = note.Text;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingNote = null;
        NoteText = string.Empty;
        if (SelectedBook is { } selectedBook)
        {
            _ = LoadReferenceAsync(selectedBook, 1, 1, refreshChapters: true);
        }

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

    partial void OnSelectedBookChanged(BookInfo? value)
    {
        if (!_suppressReferenceUpdates && value is not null)
        {
            _ = LoadReferenceAsync(value, 1, 1, refreshChapters: true);
        }
    }

    partial void OnSelectedChapterChanged(int value)
    {
        if (!_suppressReferenceUpdates && SelectedBook is { } selectedBook && value > 0)
        {
            _ = LoadReferenceAsync(selectedBook, value, 1, refreshChapters: false);
        }
    }

    private async Task LoadReferenceAsync(
        BookInfo book,
        int chapter,
        int verse,
        bool refreshChapters)
    {
        var requestVersion = ++_referenceLoadVersion;
        IsReferenceLoading = true;
        ErrorMessage = null;
        try
        {
            if (chapter < 1 || chapter > book.ChapterCount)
            {
                ErrorMessage = Localizer.Get("A referência escolhida não está disponível na Bíblia.");
                return;
            }

            _suppressReferenceUpdates = true;
            if (refreshChapters)
            {
                SelectedChapter = 0;
                Chapters = new ObservableCollection<int>(Enumerable.Range(1, book.ChapterCount));
                SelectedChapter = chapter;
            }

            SelectedVerse = 0;
            Verses = [];
            _suppressReferenceUpdates = false;

            var chapterVerses = await bibleLibrary.GetChapterAsync(book.BookId, chapter);
            if (requestVersion != _referenceLoadVersion ||
                SelectedBook?.BookId != book.BookId ||
                SelectedChapter != chapter)
            {
                return;
            }

            Verses = new ObservableCollection<int>(chapterVerses.Select(item => item.Verse));
            if (Verses.Count == 0)
            {
                ErrorMessage = Localizer.Get("Este capítulo não está disponível.");
                return;
            }

            _suppressReferenceUpdates = true;
            SelectedVerse = Verses.Contains(verse) ? verse : Verses[0];
            _suppressReferenceUpdates = false;
        }
        catch (InvalidDataException exception)
        {
            logger.LogError(exception, "The Bible data source returned invalid reference data.");
            ErrorMessage = Localizer.Get("O banco da Bíblia está inválido. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not load note reference options.");
            ErrorMessage = Localizer.Get("Não foi possível carregar as opções de referência. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Could not query note reference options.");
            ErrorMessage = Localizer.Get("Não foi possível carregar as opções de referência. Tente novamente.");
        }
        finally
        {
            if (requestVersion == _referenceLoadVersion)
            {
                _suppressReferenceUpdates = false;
                IsReferenceLoading = false;
            }
        }
    }
}
