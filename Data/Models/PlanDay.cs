using BibleNotify.Localization;

namespace BibleNotify.Data.Models;

public sealed record PlanDay(
    int DayNumber,
    int BookId,
    string Reference,
    int Chapter,
    bool IsCompleted)
{
    public string DayLabel => Localizer.Format("Dia {0}", DayNumber);
}
