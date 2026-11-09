namespace BibleNotify.Data.Models;

public sealed record AccountRegistration(
    string FirstName,
    string LastName,
    string MinistryRole,
    string Phone,
    string Email,
    string Password);
