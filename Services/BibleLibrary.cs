using BibleNotify.Data.Models;
using BibleNotify.Localization;
using Microsoft.Data.Sqlite;

namespace BibleNotify.Services;

public sealed class BibleLibrary : IBibleLibrary
{
    private const string DatabaseAssetName = "bible.db";
    private const string DatabaseFileName = "bible-porbr2018.db";

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private string? _databasePath;
    private int _verseCount;

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
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var bookCode = reader.GetString(1);
            books.Add(new BookInfo(
                reader.GetInt32(0),
                bookCode,
                Localizer.GetBibleBookName(bookCode, reader.GetString(2)),
                reader.GetString(3),
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
