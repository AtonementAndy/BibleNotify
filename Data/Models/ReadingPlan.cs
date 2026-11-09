using BibleNotify.Localization;

namespace BibleNotify.Data.Models;

public sealed record ReadingPlan(
    int PlanId,
    string Title,
    string Description,
    int TotalDays,
    int CompletedDays,
    bool IsActive)
{
    public bool IsSelected { get; init; }

    public string DaysLabel => Localizer.Format("{0} dias", TotalDays);
}
