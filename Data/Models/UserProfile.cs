using BibleNotify.Localization;

namespace BibleNotify.Data.Models;

public sealed record UserProfile(
    int UserId,
    string FirstName,
    string LastName,
    string MinistryRole,
    string Phone,
    string Email,
    bool IsPremiumPreview,
    string Theme,
    string Language)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string MembershipName => IsPremiumPreview
        ? Localizer.Get("Premium (prévia)")
        : Localizer.Get("Conta gratuita");
}
