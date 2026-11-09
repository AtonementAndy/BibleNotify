using BibleNotify.Data.Models;

namespace BibleNotify.Services;

public interface IBibleLibrary
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookInfo>> GetBooksAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BibleVerse>> GetChapterAsync(
        int bookId,
        int chapter,
        CancellationToken cancellationToken = default);

    Task<BibleVerse?> GetVerseAsync(
        string bookCode,
        int chapter,
        int verse,
        CancellationToken cancellationToken = default);

    Task<BibleVerse?> GetRandomVerseAsync(
        int? excludedVerseId = null,
        CancellationToken cancellationToken = default);
}
