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
    string Language,
    bool HasSeenWelcome)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
