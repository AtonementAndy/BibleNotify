using System.Collections.ObjectModel;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using BibleNotify.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.ViewModels;

public partial class BibleViewModel : ObservableObject
{
    private readonly IBibleLibrary _bibleLibrary;
    private readonly IUserDataService _userData;
    private readonly ILogger<BibleViewModel> _logger;
    private bool _suppressSelectionLoad;
    private bool _selectionLoadPending;

    [ObservableProperty]
    public partial ObservableCollection<BookInfo> Books { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<int> Chapters { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<BibleVerse> Verses { get; set; }

    [ObservableProperty]
    public partial BookInfo? SelectedBook { get; set; }

    [ObservableProperty]
    public partial int SelectedChapter { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public string ChapterTitle => SelectedBook is null
        ? Localizer.Get("Bíblia")
        : $"{SelectedBook.Name} {SelectedChapter}";

    public bool CanGoPrevious =>
        SelectedBook is not null &&
        (SelectedChapter > 1 || Books.IndexOf(SelectedBook) > 0);

    public bool CanGoNext =>
        SelectedBook is not null &&
        (SelectedChapter < SelectedBook.ChapterCount || Books.IndexOf(SelectedBook) < Books.Count - 1);

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public BibleViewModel(
        IBibleLibrary bibleLibrary,
        IUserDataService userData,
        ILogger<BibleViewModel> logger)
    {
        _bibleLibrary = bibleLibrary;
        _userData = userData;
        _logger = logger;
        Books = [];
        Chapters = [];
        Verses = [];
        SelectedChapter = 1;
    }

    public async Task NavigateToReferenceAsync(int bookId, int chapter)
    {
        var book = Books.FirstOrDefault(item => item.BookId == bookId);
        if (book is null || chapter < 1 || chapter > book.ChapterCount)
        {
            ErrorMessage = Localizer.Get("A referência solicitada não está disponível.");
            return;
        }

        SetSelection(book, chapter);
        await LoadSelectedChapterAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (Books.Count > 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var availableBooks = await _bibleLibrary.GetBooksAsync();
            _suppressSelectionLoad = true;
            Books = new ObservableCollection<BookInfo>(availableBooks);
            SelectedBook = Books.FirstOrDefault();
            _suppressSelectionLoad = false;
            if (SelectedBook is not null)
            {
                await LoadSelectedChapterAsync();
            }
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "The Bible data source returned invalid data.");
            ErrorMessage = Localizer.Get("O banco da Bíblia está inválido. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Could not load the offline Bible.");
            ErrorMessage = Localizer.Get("Não foi possível carregar a Bíblia offline. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            _logger.LogError(exception, "Could not read the offline Bible.");
            ErrorMessage = Localizer.Get("Não foi possível carregar a Bíblia offline. Tente novamente.");
        }
        finally
        {
            IsBusy = false;
            LoadPendingSelection();
        }
    }

    [RelayCommand]
    private async Task LoadSelectedChapterAsync()
    {
        if (SelectedBook is null || SelectedChapter < 1)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var chapterVerses = await _bibleLibrary.GetChapterAsync(SelectedBook.BookId, SelectedChapter);
            Verses = new ObservableCollection<BibleVerse>(chapterVerses);
            if (Verses.Count == 0)
            {
                ErrorMessage = Localizer.Get("Este capítulo não está disponível.");
            }
            else if (_userData.HasPremiumAccess)
            {
                try
                {
                    await _userData.RecordBibleReadAsync();
                }
                catch (SqliteException exception)
                {
                    _logger.LogError(exception, "Could not save reading progress.");
                    ErrorMessage = Localizer.Get("Capítulo carregado, mas não foi possível salvar seu progresso.");
                }
                catch (UnauthorizedAccessException exception)
                {
                    _logger.LogWarning(exception, "Reading progress was unavailable for this account.");
                }
            }
        }
        catch (InvalidDataException exception)
        {
            _logger.LogError(exception, "The Bible data source returned invalid chapter data.");
            ErrorMessage = Localizer.Get("O banco da Bíblia está inválido. Reinstale o aplicativo.");
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Could not load the selected Bible chapter.");
            ErrorMessage = Localizer.Get("Não foi possível abrir este capítulo. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            _logger.LogError(exception, "Could not query the selected Bible chapter.");
            ErrorMessage = Localizer.Get("Não foi possível abrir este capítulo. Tente novamente.");
        }
        finally
        {
            IsBusy = false;
            LoadPendingSelection();
        }
    }

    [RelayCommand]
    private async Task GoToPreviousChapterAsync()
    {
        if (SelectedBook is null)
        {
            return;
        }

        if (SelectedChapter > 1)
        {
            SetSelection(SelectedBook, SelectedChapter - 1);
        }
        else
        {
            var index = Books.IndexOf(SelectedBook);
            if (index <= 0)
            {
                return;
            }

            SetSelection(Books[index - 1], Books[index - 1].ChapterCount);
        }

        await LoadSelectedChapterAsync();
    }

    [RelayCommand]
    private async Task GoToNextChapterAsync()
    {
        if (SelectedBook is null)
        {
            return;
        }

        if (SelectedChapter < SelectedBook.ChapterCount)
        {
            SetSelection(SelectedBook, SelectedChapter + 1);
        }
        else
        {
            var index = Books.IndexOf(SelectedBook);
            if (index >= Books.Count - 1)
            {
                return;
            }

            SetSelection(Books[index + 1], 1);
        }

        await LoadSelectedChapterAsync();
    }

    partial void OnSelectedBookChanged(BookInfo? value)
    {
        if (value is not null)
        {
            var wasSuppressed = _suppressSelectionLoad;
            _suppressSelectionLoad = true;
            Chapters = new ObservableCollection<int>(Enumerable.Range(1, value.ChapterCount));
            SelectedChapter = 0;
            SelectedChapter = 1;
            _suppressSelectionLoad = wasSuppressed;
        }

        UpdateNavigationState();
        RequestSelectedChapterLoad();
    }

    partial void OnSelectedChapterChanged(int value)
    {
        UpdateNavigationState();
        RequestSelectedChapterLoad();
    }

    partial void OnBooksChanged(ObservableCollection<BookInfo> value) => UpdateNavigationState();

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    private void UpdateNavigationState()
    {
        OnPropertyChanged(nameof(ChapterTitle));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private void SetSelection(BookInfo? book, int chapter)
    {
        _suppressSelectionLoad = true;
        SelectedBook = book;
        SelectedChapter = chapter;
        _suppressSelectionLoad = false;
    }

    private void RequestSelectedChapterLoad()
    {
        if (_suppressSelectionLoad || SelectedBook is null || Books.Count == 0)
        {
            return;
        }

        if (IsBusy)
        {
            _selectionLoadPending = true;
            return;
        }

        LoadSelectedChapterCommand.Execute(null);
    }

    private void LoadPendingSelection()
    {
        if (!_selectionLoadPending)
        {
            return;
        }

        _selectionLoadPending = false;
        RequestSelectedChapterLoad();
    }
}
