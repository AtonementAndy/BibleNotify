using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using Microsoft.Data.Sqlite;

namespace BibleNotify.Services;

public sealed class BibleLibrary : IBibleLibrary
{
    private const string DatabaseAssetName = "bible.db";
    private const string DatabaseFileName = "bible-porbr2018.db";
    private const int RandomVerseAttempts = 3;
    private static readonly TimeSpan MinimumOnlineRequestInterval = TimeSpan.FromSeconds(2.1);
    private static readonly IReadOnlyDictionary<string, string> EnglishBookCodeOverrides =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SOL"] = "SNG",
            ["EZE"] = "EZK",
            ["JOE"] = "JOL",
            ["NAH"] = "NAM",
            ["MAR"] = "MRK",
            ["JOH"] = "JHN",
            ["PHI"] = "PHP",
            ["JAM"] = "JAS",
            ["1JO"] = "1JN",
            ["2JO"] = "2JN",
            ["3JO"] = "3JN"
        };
    private static readonly IReadOnlyDictionary<string, string> LocalBookCodeOverrides =
        EnglishBookCodeOverrides.ToDictionary(
            item => item.Value,
            item => item.Key,
            StringComparer.Ordinal);

    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly SemaphoreSlim _onlineRequestGate = new(1, 1);
    private readonly ConcurrentDictionary<(int BookId, int Chapter), IReadOnlyList<BibleVerse>> _onlineChapters = new();
    private string? _databasePath;
    private int _verseCount;
    private DateTimeOffset _nextOnlineRequestAt;

    public BibleLibrary(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_databasePath is not null)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_databasePath is not null)
            {
                return;
            }

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
            if (!File.Exists(databasePath))
            {
                var temporaryPath = $"{databasePath}.tmp";
                await using (var bundledDatabase = await FileSystem
                    .OpenAppPackageFileAsync(DatabaseAssetName)
                    .ConfigureAwait(false))
                {
                    await using var localDatabase = new FileStream(
                        temporaryPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 64 * 1024,
                        useAsync: true);
                    await bundledDatabase
                        .CopyToAsync(localDatabase, cancellationToken)
                        .ConfigureAwait(false);
                }

                File.Move(temporaryPath, databasePath);
            }

            var verseCount = await Task.Run(() =>
            {
                using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared
                }.ToString());
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT
                        (SELECT COUNT(*) FROM Books),
                        (SELECT COUNT(*) FROM Verses);
                    """;
                using var reader = command.ExecuteReader();
                if (!reader.Read() || reader.GetInt32(0) != 66 || reader.GetInt32(1) != 31_102)
                {
                    throw new InvalidDataException("The bundled Bible database failed its book and verse count check.");
                }

                return reader.GetInt32(1);
            }, cancellationToken).ConfigureAwait(false);

            _databasePath = databasePath;
            _verseCount = verseCount;
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    public async Task<IReadOnlyList<BookInfo>> GetBooksAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT BookId, Code, Name, Testament, BookOrder, ChapterCount
            FROM Books
            ORDER BY BookOrder;
            """;

        var books = new List<BookInfo>(66);
        var isPortuguese = Localizer.IsPortuguese;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var localBookCode = reader.GetString(1);
            var bookCode = isPortuguese
                ? localBookCode
                : EnglishBookCodeOverrides.GetValueOrDefault(localBookCode, localBookCode);
            var testament = reader.GetString(3);
            books.Add(new BookInfo(
                reader.GetInt32(0),
                localBookCode,
                Localizer.GetBibleBookName(bookCode, reader.GetString(2)),
                isPortuguese
                    ? testament
                    : testament == "Antigo" ? "Old Testament" : "New Testament",
                reader.GetInt32(4),
                reader.GetInt32(5)));
        }

        return books;
    }

    public async Task<IReadOnlyList<BibleVerse>> GetChapterAsync(
        int bookId,
        int chapter,
        CancellationToken cancellationToken = default)
    {
        if (!Localizer.IsPortuguese)
        {
            return await GetOnlineChapterAsync(bookId, chapter, cancellationToken).ConfigureAwait(false);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT VerseId, BookId, Chapter, Verse, Text
            FROM Verses
            WHERE BookId = $bookId AND Chapter = $chapter
            ORDER BY Verse;
            """;
        command.Parameters.AddWithValue("$bookId", bookId);
        command.Parameters.AddWithValue("$chapter", chapter);

        var verses = new List<BibleVerse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            verses.Add(new BibleVerse(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4)));
        }

        return verses;
    }

    public async Task<BibleVerse?> GetVerseAsync(
        string bookCode,
        int chapter,
        int verse,
        CancellationToken cancellationToken = default)
    {
        if (!Localizer.IsPortuguese)
        {
            var localBookCode = LocalBookCodeOverrides.GetValueOrDefault(bookCode, bookCode);
            var book = (await GetBooksAsync(cancellationToken).ConfigureAwait(false))
                .FirstOrDefault(item => item.Code.Equals(localBookCode, StringComparison.OrdinalIgnoreCase));
            if (book is null || chapter < 1 || chapter > book.ChapterCount || verse < 1)
            {
                return null;
            }

            var chapterVerses = await GetChapterAsync(book.BookId, chapter, cancellationToken).ConfigureAwait(false);
            return chapterVerses.FirstOrDefault(item => item.Verse == verse);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Verses.VerseId, Verses.BookId, Verses.Chapter, Verses.Verse, Verses.Text
            FROM Verses
            INNER JOIN Books ON Books.BookId = Verses.BookId
            WHERE Books.Code = $bookCode AND Verses.Chapter = $chapter AND Verses.Verse = $verse
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$bookCode", bookCode);
        command.Parameters.AddWithValue("$chapter", chapter);
        command.Parameters.AddWithValue("$verse", verse);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new BibleVerse(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetInt32(2),
            reader.GetInt32(3),
            reader.GetString(4));
    }

    public async Task<BibleVerse?> GetRandomVerseAsync(
        int? excludedVerseId = null,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (!Localizer.IsPortuguese)
        {
            var books = await GetBooksAsync(cancellationToken).ConfigureAwait(false);
            if (Localizer.IsSpanish)
            {
                var attempts = excludedVerseId.HasValue ? RandomVerseAttempts : 1;
                for (var attempt = 0; attempt < attempts; attempt++)
                {
                    var book = books[Random.Shared.Next(books.Count)];
                    var chapter = Random.Shared.Next(1, book.ChapterCount + 1);
                    var verses = await GetOnlineChapterAsync(book.BookId, chapter, cancellationToken)
                        .ConfigureAwait(false);
                    var candidates = verses
                        .Where(verse => verse.VerseId != excludedVerseId)
                        .ToArray();
                    if (candidates.Length > 0)
                    {
                        return candidates[Random.Shared.Next(candidates.Length)];
                    }
                }

                return null;
            }

            for (var attempt = 0; attempt < (excludedVerseId.HasValue ? RandomVerseAttempts : 1); attempt++)
            {
                var response = await GetOnlineDataAsync<OnlineRandomVerseResponse>(
                    "data/web/random",
                    cancellationToken).ConfigureAwait(false);
                var verse = response.RandomVerse
                    ?? throw new InvalidDataException("The online Bible service returned no random verse.");
                var localBookCode = LocalBookCodeOverrides.GetValueOrDefault(verse.BookId, verse.BookId);
                var book = books.FirstOrDefault(item =>
                    item.Code.Equals(localBookCode, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("The online Bible service returned an unknown book.");
                var mappedVerse = MapOnlineVerse(book, verse);
                if (mappedVerse.VerseId != excludedVerseId)
                {
                    return mappedVerse;
                }
            }

            return null;
        }

        var availableCount = _verseCount - (excludedVerseId.HasValue ? 1 : 0);
        if (availableCount <= 0)
        {
            return null;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = excludedVerseId.HasValue
            ? """
              SELECT VerseId, BookId, Chapter, Verse, Text
              FROM Verses
              WHERE VerseId <> $excludedVerseId
              ORDER BY VerseId
              LIMIT 1 OFFSET $offset;
              """
            : """
              SELECT VerseId, BookId, Chapter, Verse, Text
              FROM Verses
              ORDER BY VerseId
              LIMIT 1 OFFSET $offset;
              """;
        if (excludedVerseId.HasValue)
        {
            command.Parameters.AddWithValue("$excludedVerseId", excludedVerseId.Value);
        }

        command.Parameters.AddWithValue("$offset", Random.Shared.Next(availableCount));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new BibleVerse(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetString(4));
    }

    private async Task<IReadOnlyList<BibleVerse>> GetOnlineChapterAsync(
        int bookId,
        int chapter,
        CancellationToken cancellationToken)
    {
        var book = (await GetBooksAsync(cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(item => item.BookId == bookId);
        if (book is null || chapter < 1 || chapter > book.ChapterCount)
        {
            throw new ArgumentOutOfRangeException(nameof(chapter), Localizer.Get("A referência solicitada não está disponível."));
        }

        if (_onlineChapters.TryGetValue((book.BookId, chapter), out var cachedChapter))
        {
            return cachedChapter;
        }

        var requestUri = Localizer.IsSpanish
            ? $"{Uri.EscapeDataString(GetApiBookCode(book.Code))}+{chapter}?translation=rv1909"
            : $"data/web/{Uri.EscapeDataString(GetApiBookCode(book.Code))}/{chapter}";
        var response = await GetOnlineDataAsync<OnlineChapter>(
            requestUri,
            cancellationToken).ConfigureAwait(false);
        if (response.Verses is null || response.Verses.Length == 0)
        {
            throw new InvalidDataException("The online Bible service returned an empty chapter.");
        }

        var verses = response.Verses
            .Select(verse => verse is null
                ? throw new InvalidDataException("The online Bible service returned an empty verse.")
                : MapOnlineVerse(book, verse, chapter))
            .ToArray();
        return _onlineChapters.GetOrAdd((book.BookId, chapter), verses);
    }

    private async Task<T> GetOnlineDataAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        await _onlineRequestGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var delay = _nextOnlineRequestAt - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            using var response = await _httpClient.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            _nextOnlineRequestAt = DateTimeOffset.UtcNow + MinimumOnlineRequestInterval;
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is > 262_144)
            {
                throw new InvalidDataException("The online Bible service returned an oversized response.");
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new InvalidDataException("The online Bible service returned an empty response.");
        }
        finally
        {
            _onlineRequestGate.Release();
        }
    }

    private static BibleVerse MapOnlineVerse(BookInfo book, OnlineVerse verse, int? expectedChapter = null)
    {
        if (!string.Equals(
                verse.BookId,
                GetApiBookCode(book.Code),
                StringComparison.OrdinalIgnoreCase) ||
            verse.Chapter < 1 ||
            verse.Chapter > book.ChapterCount ||
            verse.Verse < 1 ||
            string.IsNullOrWhiteSpace(verse.Text) ||
            (expectedChapter.HasValue && verse.Chapter != expectedChapter.Value))
        {
            throw new InvalidDataException("The online Bible service returned an invalid verse.");
        }

        var verseId = checked(book.BookId * 1_000_000 + verse.Chapter * 1_000 + verse.Verse);
        return new BibleVerse(verseId, book.BookId, verse.Chapter, verse.Verse, verse.Text);
    }

    private static string GetApiBookCode(string localBookCode) =>
        EnglishBookCodeOverrides.GetValueOrDefault(localBookCode, localBookCode);

    private sealed record OnlineChapter(
        [property: JsonPropertyName("verses")] OnlineVerse[]? Verses);

    private sealed record OnlineRandomVerseResponse(
        [property: JsonPropertyName("random_verse")] OnlineVerse? RandomVerse);

    private sealed record OnlineVerse(
        [property: JsonPropertyName("book_id")] string BookId,
        [property: JsonPropertyName("chapter")] int Chapter,
        [property: JsonPropertyName("verse")] int Verse,
        [property: JsonPropertyName("text")] string Text);

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared
        }.ToString());

        await Task.Run(connection.Open, cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
