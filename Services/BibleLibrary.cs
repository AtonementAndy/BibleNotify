using BibleNotify.Data.Models;
using BibleNotify.Localization;
using Microsoft.Data.Sqlite;

namespace BibleNotify.Services;

public sealed class BibleLibrary : IBibleLibrary
{
    private const string DatabaseAssetName = "bible.db";
    private const string DatabaseFileName = "bible-multilingual.db";
    private const int ExpectedBookCount = 66;
    private const int DatabaseSchemaVersion = 2;
    private static readonly string[] ExpectedLanguages = ["pt-BR", "en", "es"];
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

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private string? _databasePath;

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

            await Task.Run(
                () => ValidateDatabase(databasePath),
                cancellationToken).ConfigureAwait(false);

            _databasePath = databasePath;
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
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Verses.VerseId, Verses.BookId, Verses.Chapter, Verses.Verse, BibleTexts.Text
            FROM Verses
            INNER JOIN BibleTexts ON BibleTexts.VerseId = Verses.VerseId
            WHERE Verses.BookId = $bookId
                AND Verses.Chapter = $chapter
                AND BibleTexts.LanguageCode = $languageCode
            ORDER BY Verses.Verse;
            """;
        command.Parameters.AddWithValue("$bookId", bookId);
        command.Parameters.AddWithValue("$chapter", chapter);
        command.Parameters.AddWithValue("$languageCode", Localizer.BibleLanguageCode);

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
            SELECT Verses.VerseId, Verses.BookId, Verses.Chapter, Verses.Verse, BibleTexts.Text
            FROM Books
            INNER JOIN Verses ON Verses.BookId = Books.BookId
            INNER JOIN BibleTexts ON BibleTexts.VerseId = Verses.VerseId
            WHERE Books.Code = $bookCode AND Verses.Chapter = $chapter AND Verses.Verse = $verse
                AND BibleTexts.LanguageCode = $languageCode
            LIMIT 1;
            """;
        command.Parameters.AddWithValue(
            "$bookCode",
            LocalBookCodeOverrides.GetValueOrDefault(bookCode, bookCode));
        command.Parameters.AddWithValue("$chapter", chapter);
        command.Parameters.AddWithValue("$verse", verse);
        command.Parameters.AddWithValue("$languageCode", Localizer.BibleLanguageCode);

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
        var languageCode = Localizer.BibleLanguageCode;
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = """
            SELECT COUNT(*)
            FROM BibleTexts
            WHERE LanguageCode = $languageCode
                AND ($excludedVerseId IS NULL OR VerseId <> $excludedVerseId);
            """;
        countCommand.Parameters.AddWithValue("$languageCode", languageCode);
        countCommand.Parameters.AddWithValue(
            "$excludedVerseId",
            excludedVerseId is null ? DBNull.Value : excludedVerseId.Value);
        var availableCount = Convert.ToInt32(
            await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (availableCount <= 0)
        {
            return null;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Verses.VerseId, Verses.BookId, Verses.Chapter, Verses.Verse, BibleTexts.Text
            FROM BibleTexts
            INNER JOIN Verses ON Verses.VerseId = BibleTexts.VerseId
            WHERE BibleTexts.LanguageCode = $languageCode
                AND ($excludedVerseId IS NULL OR BibleTexts.VerseId <> $excludedVerseId)
            ORDER BY BibleTexts.VerseId
            LIMIT 1 OFFSET $offset;
            """;
        command.Parameters.AddWithValue("$languageCode", languageCode);
        command.Parameters.AddWithValue(
            "$excludedVerseId",
            excludedVerseId is null ? DBNull.Value : excludedVerseId.Value);
        command.Parameters.AddWithValue("$offset", Random.Shared.Next(availableCount));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return new BibleVerse(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetString(4));
    }

    private static void ValidateDatabase(string databasePath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared
        }.ToString());
        connection.Open();

        using (var versionCommand = connection.CreateCommand())
        {
            versionCommand.CommandText = "PRAGMA user_version;";
            if (Convert.ToInt32(versionCommand.ExecuteScalar()) != DatabaseSchemaVersion)
            {
                throw new InvalidDataException("The bundled Bible database has an unsupported schema version.");
            }
        }

        using (var countsCommand = connection.CreateCommand())
        {
            countsCommand.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM Books),
                    (SELECT COUNT(*) FROM Verses),
                    (SELECT COUNT(*) FROM BibleTranslations);
                """;
            using var reader = countsCommand.ExecuteReader();
            if (!reader.Read() ||
                reader.GetInt32(0) != ExpectedBookCount ||
                reader.GetInt32(1) < 31_102 ||
                reader.GetInt32(2) != ExpectedLanguages.Length)
            {
                throw new InvalidDataException("The bundled Bible database failed its coverage check.");
            }
        }

        foreach (var languageCode in ExpectedLanguages)
        {
            using var languageCommand = connection.CreateCommand();
            languageCommand.CommandText = """
                SELECT BibleTranslations.VerseCount,
                    (SELECT COUNT(*)
                     FROM BibleTexts
                     WHERE BibleTexts.LanguageCode = BibleTranslations.LanguageCode),
                    (SELECT COUNT(*)
                     FROM BibleTexts
                     INNER JOIN Verses ON Verses.VerseId = BibleTexts.VerseId
                     WHERE BibleTexts.LanguageCode = BibleTranslations.LanguageCode
                         AND Verses.BookId = 1
                         AND Verses.Chapter = 1
                         AND Verses.Verse = 1
                         AND length(trim(BibleTexts.Text)) > 0)
                FROM BibleTranslations
                WHERE LanguageCode = $languageCode;
                """;
            languageCommand.Parameters.AddWithValue("$languageCode", languageCode);
            using var reader = languageCommand.ExecuteReader();
            if (!reader.Read() ||
                reader.GetInt32(0) < 31_000 ||
                reader.GetInt32(0) != reader.GetInt32(1) ||
                reader.GetInt32(2) != 1)
            {
                throw new InvalidDataException(
                    $"The bundled Bible database is incomplete for language '{languageCode}'.");
            }
        }
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
