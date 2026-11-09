using BibleNotify.Localization;

namespace BibleNotify.Data.Models;

public sealed record BibleNote(
    int NoteId,
    int BookId,
    string BookName,
    int Chapter,
    int Verse,
    string Text,
    DateTime UpdatedAt)
{
    public string Reference => $"{BookName} {Chapter}:{Verse}";

    public string UpdatedDateLabel => UpdatedAt.ToString(
        Localizer.IsPortuguese ? "dd/MM/yyyy" : "M/d/yyyy",
        System.Globalization.CultureInfo.CurrentCulture);
}
