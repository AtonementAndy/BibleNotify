using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

if (args.Length != 4)
{
    Console.Error.WriteLine(
        "Usage: BibleDatabaseBuilder <portuguese-vpl> <english-vpl> <spanish-vpl> <output-database>");
    return 2;
}

var translations = new[]
{
    new TranslationDefinition("pt-BR", "Bíblia Livre 2018", Path.GetFullPath(args[0])),
    new TranslationDefinition("en", "World English Bible Protestant Edition", Path.GetFullPath(args[1])),
    new TranslationDefinition("es", "Reina-Valera 1909", Path.GetFullPath(args[2]))
};
var databasePath = Path.GetFullPath(args[3]);

foreach (var translation in translations)
{
    if (!File.Exists(translation.SourcePath))
    {
        Console.Error.WriteLine(
            $"The {translation.LanguageCode} VPL source file was not found: {translation.SourcePath}");
        return 2;
    }
}

var books = new[]
{
    new BookDefinition("GEN", "Gênesis", "Antigo"),
    new BookDefinition("EXO", "Êxodo", "Antigo"),
    new BookDefinition("LEV", "Levítico", "Antigo"),
    new BookDefinition("NUM", "Números", "Antigo"),
    new BookDefinition("DEU", "Deuteronômio", "Antigo"),
    new BookDefinition("JOS", "Josué", "Antigo"),
    new BookDefinition("JDG", "Juízes", "Antigo"),
    new BookDefinition("RUT", "Rute", "Antigo"),
    new BookDefinition("1SA", "1 Samuel", "Antigo"),
    new BookDefinition("2SA", "2 Samuel", "Antigo"),
    new BookDefinition("1KI", "1 Reis", "Antigo"),
    new BookDefinition("2KI", "2 Reis", "Antigo"),
    new BookDefinition("1CH", "1 Crônicas", "Antigo"),
    new BookDefinition("2CH", "2 Crônicas", "Antigo"),
    new BookDefinition("EZR", "Esdras", "Antigo"),
    new BookDefinition("NEH", "Neemias", "Antigo"),
    new BookDefinition("EST", "Ester", "Antigo"),
    new BookDefinition("JOB", "Jó", "Antigo"),
    new BookDefinition("PSA", "Salmos", "Antigo"),
    new BookDefinition("PRO", "Provérbios", "Antigo"),
    new BookDefinition("ECC", "Eclesiastes", "Antigo"),
    new BookDefinition("SOL", "Cantares", "Antigo"),
    new BookDefinition("ISA", "Isaías", "Antigo"),
    new BookDefinition("JER", "Jeremias", "Antigo"),
    new BookDefinition("LAM", "Lamentações", "Antigo"),
    new BookDefinition("EZE", "Ezequiel", "Antigo"),
    new BookDefinition("DAN", "Daniel", "Antigo"),
    new BookDefinition("HOS", "Oseias", "Antigo"),
    new BookDefinition("JOE", "Joel", "Antigo"),
    new BookDefinition("AMO", "Amós", "Antigo"),
    new BookDefinition("OBA", "Obadias", "Antigo"),
    new BookDefinition("JON", "Jonas", "Antigo"),
    new BookDefinition("MIC", "Miqueias", "Antigo"),
    new BookDefinition("NAH", "Naum", "Antigo"),
    new BookDefinition("HAB", "Habacuque", "Antigo"),
    new BookDefinition("ZEP", "Sofonias", "Antigo"),
    new BookDefinition("HAG", "Ageu", "Antigo"),
    new BookDefinition("ZEC", "Zacarias", "Antigo"),
    new BookDefinition("MAL", "Malaquias", "Antigo"),
    new BookDefinition("MAT", "Mateus", "Novo"),
    new BookDefinition("MAR", "Marcos", "Novo"),
    new BookDefinition("LUK", "Lucas", "Novo"),
    new BookDefinition("JOH", "João", "Novo"),
    new BookDefinition("ACT", "Atos", "Novo"),
    new BookDefinition("ROM", "Romanos", "Novo"),
    new BookDefinition("1CO", "1 Coríntios", "Novo"),
    new BookDefinition("2CO", "2 Coríntios", "Novo"),
    new BookDefinition("GAL", "Gálatas", "Novo"),
    new BookDefinition("EPH", "Efésios", "Novo"),
    new BookDefinition("PHI", "Filipenses", "Novo"),
    new BookDefinition("COL", "Colossenses", "Novo"),
    new BookDefinition("1TH", "1 Tessalonicenses", "Novo"),
    new BookDefinition("2TH", "2 Tessalonicenses", "Novo"),
    new BookDefinition("1TI", "1 Timóteo", "Novo"),
    new BookDefinition("2TI", "2 Timóteo", "Novo"),
    new BookDefinition("TIT", "Tito", "Novo"),
    new BookDefinition("PHM", "Filemom", "Novo"),
    new BookDefinition("HEB", "Hebreus", "Novo"),
    new BookDefinition("JAM", "Tiago", "Novo"),
    new BookDefinition("1PE", "1 Pedro", "Novo"),
    new BookDefinition("2PE", "2 Pedro", "Novo"),
    new BookDefinition("1JO", "1 João", "Novo"),
    new BookDefinition("2JO", "2 João", "Novo"),
    new BookDefinition("3JO", "3 João", "Novo"),
    new BookDefinition("JUD", "Judas", "Novo"),
    new BookDefinition("REV", "Apocalipse", "Novo")
};

var booksByCode = books
    .Select((book, index) => (book.Code, BookId: index + 1))
    .ToDictionary(item => item.Code, item => item.BookId, StringComparer.Ordinal);
var verseLinePattern = new Regex(
    @"^(?<code>[A-Z0-9]{3}) (?<chapter>\d+):(?<verse>\d+)(?: (?<text>.*))?$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
var versesByLanguage = new Dictionary<string, List<VerseData>>(StringComparer.Ordinal);
var chapterCountsByLanguage = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);

foreach (var translation in translations)
{
    var verses = new List<VerseData>(31_103);
    var references = new HashSet<VerseReference>();
    var chapterCounts = new Dictionary<string, int>(StringComparer.Ordinal);
    var sourceBookCodes = new HashSet<string>(StringComparer.Ordinal);

    foreach (var (line, lineNumber) in File.ReadLines(translation.SourcePath)
                 .Select((line, index) => (line, index + 1)))
    {
        var match = verseLinePattern.Match(line);
        if (!match.Success)
        {
            Console.Error.WriteLine(
                $"Invalid {translation.LanguageCode} VPL record at line {lineNumber}.");
            return 2;
        }

        var code = match.Groups["code"].Value;
        if (!booksByCode.TryGetValue(code, out var bookId))
        {
            Console.Error.WriteLine(
                $"Unknown {translation.LanguageCode} Bible book code at line {lineNumber}.");
            return 2;
        }

        if (!int.TryParse(match.Groups["chapter"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var chapter) ||
            !int.TryParse(match.Groups["verse"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var verse) ||
            chapter is < 1 or > 999 ||
            verse is < 1 or > 999)
        {
            Console.Error.WriteLine(
                $"Invalid {translation.LanguageCode} chapter or verse number at line {lineNumber}.");
            return 2;
        }

        var reference = new VerseReference(bookId, chapter, verse);
        if (!references.Add(reference))
        {
            Console.Error.WriteLine(
                $"Duplicate {translation.LanguageCode} verse reference at line {lineNumber}.");
            return 2;
        }

        var text = match.Groups["text"].Success
            ? match.Groups["text"].Value.Trim()
            : string.Empty;
        verses.Add(new VerseData(reference, text));
        sourceBookCodes.Add(code);
        chapterCounts[code] = Math.Max(chapterCounts.GetValueOrDefault(code), chapter);
    }

    if (verses.Count < 31_000 || sourceBookCodes.Count != books.Length)
    {
        Console.Error.WriteLine(
            $"Unexpected {translation.LanguageCode} coverage: {verses.Count} verses across {sourceBookCodes.Count} books.");
        return 2;
    }

    versesByLanguage.Add(translation.LanguageCode, verses);
    chapterCountsByLanguage.Add(translation.LanguageCode, chapterCounts);
}

var portugueseChapterCounts = chapterCountsByLanguage["pt-BR"];
foreach (var translation in translations.Skip(1))
{
    foreach (var book in books)
    {
        if (chapterCountsByLanguage[translation.LanguageCode].GetValueOrDefault(book.Code) !=
            portugueseChapterCounts.GetValueOrDefault(book.Code))
        {
            Console.Error.WriteLine(
                $"Chapter coverage for {translation.LanguageCode} does not match the Portuguese reference.");
            return 2;
        }
    }
}

var allReferences = versesByLanguage.Values
    .SelectMany(verses => verses)
    .Select(verse => verse.Reference)
    .Distinct()
    .OrderBy(reference => reference.BookId)
    .ThenBy(reference => reference.Chapter)
    .ThenBy(reference => reference.Verse)
    .ToArray();
var verseIds = allReferences.ToDictionary(reference => reference, GetStableVerseId);
var temporaryDatabasePath = $"{databasePath}.{Guid.NewGuid():N}.tmp";

try
{
    Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
    using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
    {
        DataSource = temporaryDatabasePath,
        Pooling = false
    }.ToString()))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            using var createTables = connection.CreateCommand();
            createTables.Transaction = transaction;
            createTables.CommandText = """
                PRAGMA foreign_keys = ON;
                PRAGMA user_version = 2;
                CREATE TABLE Books (
                    BookId INTEGER NOT NULL PRIMARY KEY,
                    Code TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Testament TEXT NOT NULL,
                    BookOrder INTEGER NOT NULL,
                    ChapterCount INTEGER NOT NULL
                );
                CREATE TABLE BibleTranslations (
                    LanguageCode TEXT NOT NULL PRIMARY KEY,
                    TranslationName TEXT NOT NULL,
                    VerseCount INTEGER NOT NULL CHECK (VerseCount > 0)
                );
                CREATE TABLE Verses (
                    VerseId INTEGER NOT NULL PRIMARY KEY,
                    BookId INTEGER NOT NULL REFERENCES Books(BookId),
                    Chapter INTEGER NOT NULL CHECK (Chapter BETWEEN 1 AND 999),
                    Verse INTEGER NOT NULL CHECK (Verse BETWEEN 1 AND 999),
                    UNIQUE (BookId, Chapter, Verse)
                );
                CREATE TABLE BibleTexts (
                    LanguageCode TEXT NOT NULL REFERENCES BibleTranslations(LanguageCode),
                    VerseId INTEGER NOT NULL REFERENCES Verses(VerseId),
                    Text TEXT NOT NULL,
                    PRIMARY KEY (LanguageCode, VerseId)
                );
                CREATE INDEX IX_Verses_Book_Chapter_Verse
                    ON Verses (BookId, Chapter, Verse);
                """;
            createTables.ExecuteNonQuery();

            using var insertBook = connection.CreateCommand();
            insertBook.Transaction = transaction;
            insertBook.CommandText = """
                INSERT INTO Books (BookId, Code, Name, Testament, BookOrder, ChapterCount)
                VALUES ($bookId, $code, $name, $testament, $bookOrder, $chapterCount);
                """;
            var bookIdParameter = insertBook.Parameters.Add("$bookId", SqliteType.Integer);
            var codeParameter = insertBook.Parameters.Add("$code", SqliteType.Text);
            var nameParameter = insertBook.Parameters.Add("$name", SqliteType.Text);
            var testamentParameter = insertBook.Parameters.Add("$testament", SqliteType.Text);
            var orderParameter = insertBook.Parameters.Add("$bookOrder", SqliteType.Integer);
            var chapterCountParameter = insertBook.Parameters.Add("$chapterCount", SqliteType.Integer);

            for (var index = 0; index < books.Length; index++)
            {
                var book = books[index];
                bookIdParameter.Value = index + 1;
                codeParameter.Value = book.Code;
                nameParameter.Value = book.Name;
                testamentParameter.Value = book.Testament;
                orderParameter.Value = index + 1;
                chapterCountParameter.Value = portugueseChapterCounts[book.Code];
                insertBook.ExecuteNonQuery();
            }

            using var insertTranslation = connection.CreateCommand();
            insertTranslation.Transaction = transaction;
            insertTranslation.CommandText = """
                INSERT INTO BibleTranslations (LanguageCode, TranslationName, VerseCount)
                VALUES ($languageCode, $translationName, $verseCount);
                """;
            var translationLanguageParameter = insertTranslation.Parameters.Add("$languageCode", SqliteType.Text);
            var translationNameParameter = insertTranslation.Parameters.Add("$translationName", SqliteType.Text);
            var translationVerseCountParameter = insertTranslation.Parameters.Add("$verseCount", SqliteType.Integer);

            foreach (var translation in translations)
            {
                translationLanguageParameter.Value = translation.LanguageCode;
                translationNameParameter.Value = translation.TranslationName;
                translationVerseCountParameter.Value = versesByLanguage[translation.LanguageCode].Count;
                insertTranslation.ExecuteNonQuery();
            }

            using var insertVerse = connection.CreateCommand();
            insertVerse.Transaction = transaction;
            insertVerse.CommandText = """
                INSERT INTO Verses (VerseId, BookId, Chapter, Verse)
                VALUES ($verseId, $bookId, $chapter, $verse);
                """;
            var verseIdParameter = insertVerse.Parameters.Add("$verseId", SqliteType.Integer);
            var verseBookIdParameter = insertVerse.Parameters.Add("$bookId", SqliteType.Integer);
            var verseChapterParameter = insertVerse.Parameters.Add("$chapter", SqliteType.Integer);
            var verseNumberParameter = insertVerse.Parameters.Add("$verse", SqliteType.Integer);

            foreach (var reference in allReferences)
            {
                verseIdParameter.Value = verseIds[reference];
                verseBookIdParameter.Value = reference.BookId;
                verseChapterParameter.Value = reference.Chapter;
                verseNumberParameter.Value = reference.Verse;
                insertVerse.ExecuteNonQuery();
            }

            using var insertText = connection.CreateCommand();
            insertText.Transaction = transaction;
            insertText.CommandText = """
                INSERT INTO BibleTexts (LanguageCode, VerseId, Text)
                VALUES ($languageCode, $verseId, $text);
                """;
            var textLanguageParameter = insertText.Parameters.Add("$languageCode", SqliteType.Text);
            var textVerseIdParameter = insertText.Parameters.Add("$verseId", SqliteType.Integer);
            var textParameter = insertText.Parameters.Add("$text", SqliteType.Text);

            foreach (var translation in translations)
            {
                textLanguageParameter.Value = translation.LanguageCode;
                foreach (var verse in versesByLanguage[translation.LanguageCode])
                {
                    textVerseIdParameter.Value = verseIds[verse.Reference];
                    textParameter.Value = verse.Text;
                    insertText.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        VerifyDatabase(connection, translations, versesByLanguage, verseIds);
    }

    File.Move(temporaryDatabasePath, databasePath, overwrite: true);
}
finally
{
    if (File.Exists(temporaryDatabasePath))
    {
        File.Delete(temporaryDatabasePath);
    }
}

Console.WriteLine(
    $"Created {databasePath} with {books.Length} books and " +
    $"{versesByLanguage.Values.Sum(verses => verses.Count):N0} verses across 3 translations.");
return 0;

static int GetStableVerseId(VerseReference reference) =>
    checked(reference.BookId * 1_000_000 + reference.Chapter * 1_000 + reference.Verse);

static void VerifyDatabase(
    SqliteConnection connection,
    IReadOnlyList<TranslationDefinition> translations,
    IReadOnlyDictionary<string, List<VerseData>> versesByLanguage,
    IReadOnlyDictionary<VerseReference, int> verseIds)
{
    using (var counts = connection.CreateCommand())
    {
        counts.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM Books),
                (SELECT COUNT(*) FROM BibleTranslations),
                (SELECT COUNT(*) FROM Verses),
                (SELECT COUNT(*) FROM BibleTexts);
            """;
        using var reader = counts.ExecuteReader();
        if (!reader.Read() ||
            reader.GetInt32(0) != 66 ||
            reader.GetInt32(1) != translations.Count ||
            reader.GetInt32(2) < 31_102 ||
            reader.GetInt32(3) != versesByLanguage.Values.Sum(verses => verses.Count))
        {
            throw new InvalidDataException("The generated Bible database failed its coverage check.");
        }
    }

    foreach (var translation in translations)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT BibleTranslations.VerseCount,
                (SELECT COUNT(*)
                 FROM BibleTexts
                 WHERE BibleTexts.LanguageCode = BibleTranslations.LanguageCode),
                (SELECT Text
                 FROM BibleTexts
                 WHERE BibleTexts.LanguageCode = BibleTranslations.LanguageCode
                     AND BibleTexts.VerseId = $genesisOneOne)
            FROM BibleTranslations
            WHERE LanguageCode = $languageCode;
            """;
        command.Parameters.AddWithValue("$genesisOneOne", verseIds[new VerseReference(1, 1, 1)]);
        command.Parameters.AddWithValue("$languageCode", translation.LanguageCode);
        using var reader = command.ExecuteReader();
        if (!reader.Read() ||
            reader.GetInt32(0) != versesByLanguage[translation.LanguageCode].Count ||
            reader.GetInt32(1) != reader.GetInt32(0) ||
            string.IsNullOrWhiteSpace(reader.GetString(2)))
        {
            throw new InvalidDataException(
                $"The generated Bible database failed verification for '{translation.LanguageCode}'.");
        }

        if (translation.LanguageCode == "pt-BR" &&
            reader.GetString(2) != "No princípio criou Deus os céus e a terra.")
        {
            throw new InvalidDataException("The generated Portuguese Bible text failed verification.");
        }
    }

    using (var integrityCheck = connection.CreateCommand())
    {
        integrityCheck.CommandText = "PRAGMA integrity_check;";
        if (!string.Equals(integrityCheck.ExecuteScalar()?.ToString(), "ok", StringComparison.Ordinal))
        {
            throw new InvalidDataException("SQLite integrity check failed.");
        }
    }

    using (var foreignKeyCheck = connection.CreateCommand())
    {
        foreignKeyCheck.CommandText = "PRAGMA foreign_key_check;";
        using var reader = foreignKeyCheck.ExecuteReader();
        if (reader.Read())
        {
            throw new InvalidDataException("SQLite foreign key verification failed.");
        }
    }
}

internal sealed record TranslationDefinition(
    string LanguageCode,
    string TranslationName,
    string SourcePath);

internal sealed record BookDefinition(string Code, string Name, string Testament);

internal sealed record VerseData(VerseReference Reference, string Text);

internal readonly record struct VerseReference(int BookId, int Chapter, int Verse);
