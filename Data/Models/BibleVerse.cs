namespace BibleNotify.Data.Models;

public sealed record BibleVerse(
    int VerseId,
    int BookId,
    int Chapter,
    int Verse,
    string Text);
