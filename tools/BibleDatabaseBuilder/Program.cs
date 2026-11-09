using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: BibleDatabaseBuilder <vpl-text-file> <output-database>");
    return 2;
}

var sourcePath = Path.GetFullPath(args[0]);
var databasePath = Path.GetFullPath(args[1]);
if (!File.Exists(sourcePath))
{
    Console.Error.WriteLine($"Bible text file not found: {sourcePath}");
    return 2;
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

var definitionsByCode = books.ToDictionary(book => book.Code, StringComparer.Ordinal);
var parsedVerses = new List<VerseData>(31_102);
var verseLinePattern = new Regex(
    @"^(?<code>[A-Z0-9]{3}) (?<chapter>\d+):(?<verse>\d+) (?<text>.+)$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
var chapterCounts = new Dictionary<string, int>(StringComparer.Ordinal);

foreach (var (line, lineNumber) in File.ReadLines(sourcePath).Select((line, index) => (line, index + 1)))
{
    var match = verseLinePattern.Match(line);
    if (!match.Success)
    {
        Console.Error.WriteLine($"Invalid VPL record at line {lineNumber}.");
        return 2;
    }

    var code = match.Groups["code"].Value;
    if (!definitionsByCode.TryGetValue(code, out var book))
    {
        Console.Error.WriteLine($"Unknown Bible book code '{code}' at line {lineNumber}.");
        return 2;
    }

    var chapter = int.Parse(match.Groups["chapter"].Value);
    var verse = int.Parse(match.Groups["verse"].Value);
    parsedVerses.Add(new VerseData(
        Array.IndexOf(books, book) + 1,
        chapter,
        verse,
        match.Groups["text"].Value.Trim()));
    chapterCounts[code] = Math.Max(chapterCounts.GetValueOrDefault(code), chapter);
}

var sourceBookCodes = parsedVerses
    .Select(verse => books[verse.BookId - 1].Code)
    .Distinct(StringComparer.Ordinal)
    .ToHashSet(StringComparer.Ordinal);
if (parsedVerses.Count != 31_102 || sourceBookCodes.Count != books.Length)
{
    Console.Error.WriteLine(
        $"Unexpected Bible coverage: {parsedVerses.Count} verses across {sourceBookCodes.Count} books.");
    return 2;
}

Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
if (File.Exists(databasePath))
{
    File.Delete(databasePath);
}

using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
{
    DataSource = databasePath
}.ToString());
connection.Open();

using (var transaction = connection.BeginTransaction())
{
    using var createTables = connection.CreateCommand();
    createTables.Transaction = transaction;
    createTables.CommandText = """
        CREATE TABLE Books (
            BookId INTEGER NOT NULL PRIMARY KEY,
            Code TEXT NOT NULL UNIQUE,
            Name TEXT NOT NULL,
            Testament TEXT NOT NULL,
            BookOrder INTEGER NOT NULL,
            ChapterCount INTEGER NOT NULL
        );
        CREATE TABLE Verses (
            VerseId INTEGER NOT NULL PRIMARY KEY,
            BookId INTEGER NOT NULL REFERENCES Books(BookId),
            Chapter INTEGER NOT NULL,
            Verse INTEGER NOT NULL,
            Text TEXT NOT NULL,
            UNIQUE (BookId, Chapter, Verse)
        );
        CREATE INDEX IX_Verses_Book_Chapter ON Verses (BookId, Chapter, Verse);
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
        chapterCountParameter.Value = chapterCounts[book.Code];
        insertBook.ExecuteNonQuery();
    }

    using var insertVerse = connection.CreateCommand();
    insertVerse.Transaction = transaction;
    insertVerse.CommandText = """
        INSERT INTO Verses (BookId, Chapter, Verse, Text)
        VALUES ($bookId, $chapter, $verse, $text);
        """;
    var verseBookIdParameter = insertVerse.Parameters.Add("$bookId", SqliteType.Integer);
    var chapterParameter = insertVerse.Parameters.Add("$chapter", SqliteType.Integer);
    var verseParameter = insertVerse.Parameters.Add("$verse", SqliteType.Integer);
    var textParameter = insertVerse.Parameters.Add("$text", SqliteType.Text);

    foreach (var verse in parsedVerses)
    {
        verseBookIdParameter.Value = verse.BookId;
        chapterParameter.Value = verse.Chapter;
        verseParameter.Value = verse.Verse;
        textParameter.Value = verse.Text;
        insertVerse.ExecuteNonQuery();
    }

    transaction.Commit();
}

using var verify = connection.CreateCommand();
verify.CommandText = """
    SELECT
        (SELECT COUNT(*) FROM Books),
        (SELECT COUNT(*) FROM Verses),
        (SELECT Text FROM Verses WHERE BookId = 1 AND Chapter = 1 AND Verse = 1);
    """;
using var verificationReader = verify.ExecuteReader();
if (!verificationReader.Read())
{
    Console.Error.WriteLine("Database verification did not return a result.");
    return 1;
}

var storedBookCount = verificationReader.GetInt32(0);
var storedVerseCount = verificationReader.GetInt32(1);
var firstVerseText = verificationReader.GetString(2);
verificationReader.Close();
if (storedBookCount != books.Length || storedVerseCount != parsedVerses.Count ||
    firstVerseText != "No princípio criou Deus os céus e a terra.")
{
    Console.Error.WriteLine(
        $"Database verification failed: {storedBookCount} books, {storedVerseCount} verses, unexpected first verse.");
    return 1;
}

using var integrityCheck = connection.CreateCommand();
integrityCheck.CommandText = "PRAGMA integrity_check;";
if (!string.Equals(integrityCheck.ExecuteScalar()?.ToString(), "ok", StringComparison.Ordinal))
{
    Console.Error.WriteLine("SQLite integrity check failed.");
    return 1;
}

Console.WriteLine($"Created {databasePath} with {books.Length} books and {storedVerseCount:N0} verses.");
return 0;

internal sealed record BookDefinition(string Code, string Name, string Testament);
internal sealed record VerseData(int BookId, int Chapter, int Verse, string Text);
