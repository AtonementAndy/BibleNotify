using System.Security.Cryptography;
using System.Text;
using BibleNotify.Data.Models;
using BibleNotify.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace BibleNotify.Services;

public sealed class UserDataService(
    IBibleLibrary bibleLibrary,
    ILogger<UserDataService> logger) : IUserDataService
{
    private const int PasswordIterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MaximumPasswordLength = 1024;
    private const int MaximumEmailLength = 254;
    private const int MaximumNameLength = 100;
    private const int MaximumMinistryRoleLength = 100;
    private const int MaximumPhoneLength = 32;
    private const int MaximumNoteLength = 10_000;
    private const int MaximumReminderTitleLength = 100;
    private const int MaximumReminderMessageLength = 1000;
    private static readonly string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "user-data.db");
    private static readonly (string Title, string Message, int Hour, int Minute)[] PortugueseDefaultReminders =
    [
        ("Momento da manhã", "Comece o dia com alguns minutos na Palavra.", 8, 0),
        ("Pausa para refletir", "Reserve um instante para ler a Bíblia.", 18, 0)
    ];
    private static readonly (string Title, string Message, int Hour, int Minute)[] EnglishDefaultReminders =
    [
        ("Morning reflection", "Start your day with a few minutes in the Word.", 8, 0),
        ("Time to reflect", "Set aside a moment to read the Bible.", 18, 0)
    ];
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private bool _initialized;

    public event EventHandler? SessionChanged;

    public UserProfile? CurrentUser { get; private set; }

    public bool HasPremiumAccess
    {
        get
        {
#if DEBUG
            return CurrentUser?.IsPremiumPreview == true;
#else
            return false;
#endif
        }
    }

    public bool IsGuestSession { get; private set; }

    public bool IsFirstLoginInSession { get; private set; }

    public async Task RegisterAsync(
        AccountRegistration registration,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(registration);
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = await Task.Run(
            () => Rfc2898DeriveBytes.Pbkdf2(
                registration.Password,
                salt,
                PasswordIterations,
                HashAlgorithmName.SHA256,
                HashSize),
            cancellationToken).ConfigureAwait(false);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Users (
                Email, PasswordSalt, PasswordHash, FirstName, LastName, MinistryRole, Phone, IsPremiumPreview
            )
            VALUES ($email, $salt, $hash, $firstName, $lastName, $ministryRole, $phone, 0)
            RETURNING UserId;
            """;
        command.Parameters.AddWithValue("$email", NormalizeEmail(registration.Email));
        command.Parameters.AddWithValue("$salt", Convert.ToBase64String(salt));
        command.Parameters.AddWithValue("$hash", Convert.ToBase64String(hash));
        command.Parameters.AddWithValue("$firstName", registration.FirstName.Trim());
        command.Parameters.AddWithValue("$lastName", registration.LastName.Trim());
        command.Parameters.AddWithValue("$ministryRole", registration.MinistryRole.Trim());
        command.Parameters.AddWithValue("$phone", registration.Phone.Trim());
        var userId = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));

        await SeedDefaultRemindersAsync(connection, transaction, userId, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        CurrentUser = await GetUserAsync(userId, cancellationToken).ConfigureAwait(false);
        IsGuestSession = false;
        IsFirstLoginInSession = true;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<bool> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            email.Length > MaximumEmailLength ||
            string.IsNullOrEmpty(password) ||
            password.Length > MaximumPasswordLength)
        {
            return false;
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT UserId, PasswordSalt, PasswordHash
            FROM Users
            WHERE Email = $email;
            """;
        command.Parameters.AddWithValue("$email", NormalizeEmail(email));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var userId = reader.GetInt32(0);
        var salt = Convert.FromBase64String(reader.GetString(1));
        var expectedHash = Convert.FromBase64String(reader.GetString(2));
        reader.Close();

        var actualHash = await Task.Run(
            () => Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                PasswordIterations,
                HashAlgorithmName.SHA256,
                HashSize),
            cancellationToken).ConfigureAwait(false);
        if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
        {
            return false;
        }

        var user = await GetUserAsync(userId, cancellationToken).ConfigureAwait(false);
        CurrentUser = user;
        IsGuestSession = false;
        IsFirstLoginInSession = !user.HasSeenWelcome;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public Task ContinueAsGuestAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentUser = null;
        IsGuestSession = true;
        IsFirstLoginInSession = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentUser = null;
        IsGuestSession = false;
        IsFirstLoginInSession = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public async Task MarkWelcomeSeenAsync(CancellationToken cancellationToken = default)
    {
        var user = RequireAccount();
        if (user.HasSeenWelcome)
        {
            IsFirstLoginInSession = false;
            return;
        }

        await ExecuteUserCommandAsync(
            "UPDATE Users SET HasSeenWelcome = 1 WHERE UserId = $userId;",
            cancellationToken,
            ("$userId", user.UserId)).ConfigureAwait(false);
        CurrentUser = user with { HasSeenWelcome = true };
        IsFirstLoginInSession = false;
    }

    public async Task SetPremiumPreviewAsync(bool enabled, CancellationToken cancellationToken = default)
    {
#if !DEBUG
        cancellationToken.ThrowIfCancellationRequested();
        throw new UnauthorizedAccessException(
            Localizer.Get("A prévia Premium só fica disponível em compilações de desenvolvimento."));
#else
        var user = RequireAccount();
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = "UPDATE Users SET IsPremiumPreview = $enabled WHERE UserId = $userId;";
            command.Parameters.AddWithValue("$enabled", enabled ? 1 : 0);
            command.Parameters.AddWithValue("$userId", user.UserId);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!enabled)
        {
            await using var countCommand = connection.CreateCommand();
            countCommand.Transaction = (SqliteTransaction)transaction;
            countCommand.CommandText = "SELECT COUNT(*) FROM Reminders WHERE UserId = $userId AND IsDefault = 1;";
            countCommand.Parameters.AddWithValue("$userId", user.UserId);
            var defaultCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            var defaultReminders = GetDefaultReminders();
            foreach (var reminder in defaultReminders)
            {
                if (defaultCount >= defaultReminders.Length)
                {
                    break;
                }

                await using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = (SqliteTransaction)transaction;
                insertCommand.CommandText = """
                    INSERT INTO Reminders (UserId, Title, Message, Hour, Minute, IsEnabled, IsDefault)
                    VALUES ($userId, $title, $message, $hour, $minute, 1, 1);
                    """;
                insertCommand.Parameters.AddWithValue("$userId", user.UserId);
                insertCommand.Parameters.AddWithValue("$title", reminder.Title);
                insertCommand.Parameters.AddWithValue("$message", reminder.Message);
                insertCommand.Parameters.AddWithValue("$hour", reminder.Hour);
                insertCommand.Parameters.AddWithValue("$minute", reminder.Minute);
                await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                defaultCount++;
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        CurrentUser = user with { IsPremiumPreview = enabled };
        SessionChanged?.Invoke(this, EventArgs.Empty);
#endif
    }

    public async Task UpdateProfileAsync(
        string firstName,
        string lastName,
        string ministryRole,
        string phone,
        CancellationToken cancellationToken = default)
    {
        var user = RequireAccount();
        ValidateProfile(firstName, lastName, ministryRole, phone);

        await ExecuteUserCommandAsync(
            """
            UPDATE Users
            SET FirstName = $firstName, LastName = $lastName, MinistryRole = $ministryRole, Phone = $phone
            WHERE UserId = $userId;
            """,
            cancellationToken,
            ("$firstName", firstName.Trim()),
            ("$lastName", lastName.Trim()),
            ("$ministryRole", ministryRole.Trim()),
            ("$phone", phone?.Trim() ?? string.Empty),
            ("$userId", user.UserId)).ConfigureAwait(false);

        CurrentUser = user with
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            MinistryRole = ministryRole.Trim(),
            Phone = phone?.Trim() ?? string.Empty
        };
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetThemeAsync(string theme, CancellationToken cancellationToken = default)
    {
        var user = RequireAccount();
        if (theme is not ("Light" or "Dark" or "System"))
        {
            throw new ArgumentOutOfRangeException(nameof(theme), Localizer.Get("Tema inválido."));
        }

        await ExecuteUserCommandAsync(
            "UPDATE Users SET Theme = $theme WHERE UserId = $userId;",
            cancellationToken,
            ("$theme", theme),
            ("$userId", user.UserId)).ConfigureAwait(false);
        CurrentUser = user with { Theme = theme };
        Preferences.Set("app-theme", theme);
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RecordBibleReadAsync(CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        await ExecuteUserCommandAsync(
            """
            INSERT OR IGNORE INTO ReadingEvents (UserId, ReadDate)
            VALUES ($userId, $readDate);
            """,
            cancellationToken,
            ("$userId", user.UserId),
            ("$readDate", DateTime.Today.ToString("yyyy-MM-dd"))).ConfigureAwait(false);
    }

    public async Task<int> GetCurrentStreakAsync(CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ReadDate FROM ReadingEvents
            WHERE UserId = $userId
            ORDER BY ReadDate DESC
            LIMIT 400;
            """;
        command.Parameters.AddWithValue("$userId", user.UserId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var dates = new List<DateOnly>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (DateOnly.TryParse(reader.GetString(0), out var date))
            {
                dates.Add(date);
            }
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var expected = dates.Count > 0 && dates[0] == today ? today : today.AddDays(-1);
        var streak = 0;
        foreach (var date in dates)
        {
            if (date != expected)
            {
                break;
            }

            streak++;
            expected = expected.AddDays(-1);
        }

        return streak;
    }

    public async Task<IReadOnlyList<ReadingPlan>> GetReadingPlansAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser?.UserId;
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Plans.PlanId, Plans.Title, Plans.Description, Plans.TotalDays,
                COUNT(CASE WHEN PlanProgress.IsCompleted = 1 THEN 1 END),
                COALESCE(UserPlans.IsActive, 0)
            FROM Plans
            LEFT JOIN UserPlans ON UserPlans.PlanId = Plans.PlanId AND UserPlans.UserId = $userId
            LEFT JOIN PlanProgress ON PlanProgress.UserId = $userId AND PlanProgress.PlanId = Plans.PlanId
            GROUP BY Plans.PlanId
            ORDER BY Plans.PlanId;
            """;
        command.Parameters.AddWithValue("$userId", userId is null ? DBNull.Value : userId.Value);

        var plans = new List<ReadingPlan>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            plans.Add(new ReadingPlan(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5) == 1));
        }

        return plans;
    }

    public async Task StartReadingPlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        var books = await bibleLibrary.GetBooksAsync(cancellationToken).ConfigureAwait(false);
        var entries = BuildPlanEntries(planId, books);
        if (entries.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(planId), Localizer.Get("Plano de leitura não encontrado."));
        }

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        await using (var deactivate = connection.CreateCommand())
        {
            deactivate.Transaction = transaction;
            deactivate.CommandText = "UPDATE UserPlans SET IsActive = 0 WHERE UserId = $userId;";
            deactivate.Parameters.AddWithValue("$userId", user.UserId);
            await deactivate.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var start = connection.CreateCommand())
        {
            start.Transaction = transaction;
            start.CommandText = """
                INSERT INTO UserPlans (UserId, PlanId, IsActive, StartedAt)
                VALUES ($userId, $planId, 1, $startedAt)
                ON CONFLICT(UserId, PlanId) DO UPDATE SET IsActive = 1, StartedAt = excluded.StartedAt;
                """;
            start.Parameters.AddWithValue("$userId", user.UserId);
            start.Parameters.AddWithValue("$planId", planId);
            start.Parameters.AddWithValue("$startedAt", DateTimeOffset.UtcNow.ToString("O"));
            await start.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var clearDays = connection.CreateCommand())
        {
            clearDays.Transaction = transaction;
            clearDays.CommandText = """
                DELETE FROM PlanDays
                WHERE UserId = $userId AND PlanId = $planId;
                """;
            clearDays.Parameters.AddWithValue("$userId", user.UserId);
            clearDays.Parameters.AddWithValue("$planId", planId);
            await clearDays.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var (day, bookId, chapter) in entries)
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO PlanDays (UserId, PlanId, DayNumber, BookId, Chapter)
                VALUES ($userId, $planId, $day, $bookId, $chapter);
                """;
            insert.Parameters.AddWithValue("$userId", user.UserId);
            insert.Parameters.AddWithValue("$planId", planId);
            insert.Parameters.AddWithValue("$day", day);
            insert.Parameters.AddWithValue("$bookId", bookId);
            insert.Parameters.AddWithValue("$chapter", chapter);
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PlanDay>> GetCurrentPlanDaysAsync(
        CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        var bookNames = (await bibleLibrary.GetBooksAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(book => book.BookId, book => book.Name);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT PlanDays.DayNumber, PlanDays.BookId, PlanDays.Chapter,
                COALESCE(PlanProgress.IsCompleted, 0)
            FROM UserPlans
            INNER JOIN PlanDays ON PlanDays.UserId = UserPlans.UserId AND PlanDays.PlanId = UserPlans.PlanId
            LEFT JOIN PlanProgress ON PlanProgress.UserId = PlanDays.UserId
                AND PlanProgress.PlanId = PlanDays.PlanId AND PlanProgress.DayNumber = PlanDays.DayNumber
            WHERE UserPlans.UserId = $userId AND UserPlans.IsActive = 1
            ORDER BY PlanDays.DayNumber, PlanDays.rowid;
            """;
        command.Parameters.AddWithValue("$userId", user.UserId);

        var entries = new List<(int DayNumber, int BookId, string BookName, int Chapter, bool IsCompleted)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var bookId = reader.GetInt32(1);
            if (!bookNames.TryGetValue(bookId, out var bookName))
            {
                throw new InvalidDataException($"Reading plan references unknown Bible book {bookId}.");
            }

            entries.Add((
                reader.GetInt32(0),
                bookId,
                bookName,
                reader.GetInt32(2),
                reader.GetInt32(3) == 1));
        }

        return entries
            .GroupBy(entry => entry.DayNumber)
            .Select(day =>
            {
                var readings = day.ToArray();
                var firstReading = readings[0];
                return new PlanDay(
                    day.Key,
                    firstReading.BookId,
                    string.Join(", ", readings.Select(reading => $"{reading.BookName} {reading.Chapter}")),
                    firstReading.Chapter,
                    day.Any(reading => reading.IsCompleted));
            })
            .ToArray();
    }

    public async Task CompletePlanDayAsync(int dayNumber, CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO PlanProgress (UserId, PlanId, DayNumber, IsCompleted, CompletedAt)
            SELECT $userId, PlanId, $dayNumber, 1, $completedAt
            FROM UserPlans
            WHERE UserId = $userId AND IsActive = 1
            ON CONFLICT(UserId, PlanId, DayNumber)
            DO UPDATE SET IsCompleted = 1, CompletedAt = excluded.CompletedAt;
            """;
        command.Parameters.AddWithValue("$userId", user.UserId);
        command.Parameters.AddWithValue("$dayNumber", dayNumber);
        command.Parameters.AddWithValue("$completedAt", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<BibleNote>> GetNotesAsync(CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        var bookNames = (await bibleLibrary.GetBooksAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(book => book.BookId, book => book.Name);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Notes.NoteId, Notes.BookId, Notes.Chapter, Notes.Verse, Notes.Text, Notes.UpdatedAt
            FROM Notes
            WHERE Notes.UserId = $userId
            ORDER BY Notes.UpdatedAt DESC;
            """;
        command.Parameters.AddWithValue("$userId", user.UserId);

        var notes = new List<BibleNote>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var bookId = reader.GetInt32(1);
            if (!bookNames.TryGetValue(bookId, out var bookName))
            {
                throw new InvalidDataException($"Bible note references unknown Bible book {bookId}.");
            }

            notes.Add(new BibleNote(
                reader.GetInt32(0),
                bookId,
                bookName,
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                DateTime.Parse(reader.GetString(5))));
        }

        return notes;
    }

    public async Task SaveNoteAsync(
        int bookId,
        int chapter,
        int verse,
        string text,
        CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        ValidateNoteText(text);

        await ExecuteUserCommandAsync(
            """
            INSERT INTO Notes (UserId, BookId, Chapter, Verse, Text, UpdatedAt)
            VALUES ($userId, $bookId, $chapter, $verse, $text, $updatedAt);
            """,
            cancellationToken,
            ("$userId", user.UserId),
            ("$bookId", bookId),
            ("$chapter", chapter),
            ("$verse", verse),
            ("$text", text.Trim()),
            ("$updatedAt", DateTimeOffset.UtcNow.ToString("O"))).ConfigureAwait(false);
    }

    public async Task UpdateNoteAsync(
        int noteId,
        int bookId,
        int chapter,
        int verse,
        string text,
        CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        ValidateNoteText(text);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Notes
            SET BookId = $bookId, Chapter = $chapter, Verse = $verse,
                Text = $text, UpdatedAt = $updatedAt
            WHERE NoteId = $noteId AND UserId = $userId;
            """;
        command.Parameters.AddWithValue("$bookId", bookId);
        command.Parameters.AddWithValue("$chapter", chapter);
        command.Parameters.AddWithValue("$verse", verse);
        command.Parameters.AddWithValue("$text", text.Trim());
        command.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$noteId", noteId);
        command.Parameters.AddWithValue("$userId", user.UserId);

        if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new UnauthorizedAccessException(Localizer.Get("Você não tem permissão para editar esta anotação."));
        }
    }

    public async Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        await ExecuteUserCommandAsync(
            "DELETE FROM Notes WHERE NoteId = $noteId AND UserId = $userId;",
            cancellationToken,
            ("$noteId", noteId),
            ("$userId", user.UserId)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PrayerReminder>> GetRemindersAsync(
        CancellationToken cancellationToken = default)
    {
        var user = RequireAccount();
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ReminderId, Title, Message, Hour, Minute, IsEnabled, IsDefault
            FROM Reminders
            WHERE UserId = $userId
            ORDER BY Hour, Minute, ReminderId;
            """;
        command.Parameters.AddWithValue("$userId", user.UserId);

        var reminders = new List<PrayerReminder>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            reminders.Add(LocalizeDefaultReminder(new PrayerReminder(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5) == 1,
                reader.GetInt32(6) == 1)));
        }

        return reminders;
    }

    public async Task UpdateReminderTimeAsync(
        int reminderId,
        TimeSpan time,
        CancellationToken cancellationToken = default)
    {
        var user = RequireAccount();
        ValidateTime(time);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = user.IsPremiumPreview
            ? """
              UPDATE Reminders SET Hour = $hour, Minute = $minute
              WHERE ReminderId = $reminderId AND UserId = $userId;
              """
            : """
              UPDATE Reminders SET Hour = $hour, Minute = $minute
              WHERE ReminderId = $reminderId AND UserId = $userId AND IsDefault = 1;
              """;
        command.Parameters.AddWithValue("$hour", time.Hours);
        command.Parameters.AddWithValue("$minute", time.Minutes);
        command.Parameters.AddWithValue("$reminderId", reminderId);
        command.Parameters.AddWithValue("$userId", user.UserId);
        var updated = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (updated != 1)
        {
            throw new UnauthorizedAccessException(Localizer.Get("Somente os dois lembretes padrão podem ser alterados nesta conta."));
        }
    }

    public async Task<int> CreateReminderAsync(
        string title,
        string message,
        TimeSpan time,
        CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        ValidateReminder(title, message, time);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Reminders (UserId, Title, Message, Hour, Minute, IsEnabled, IsDefault)
            VALUES ($userId, $title, $message, $hour, $minute, 1, 0)
            RETURNING ReminderId;
            """;
        AddReminderParameters(command, user.UserId, title, message, time);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
    }

    public async Task UpdateReminderAsync(
        int reminderId,
        string title,
        string message,
        TimeSpan time,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        ValidateReminder(title, message, time);
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Reminders
            SET Title = $title, Message = $message, Hour = $hour, Minute = $minute, IsEnabled = $enabled
            WHERE ReminderId = $reminderId AND UserId = $userId;
            """;
        AddReminderParameters(command, user.UserId, title, message, time);
        command.Parameters.AddWithValue("$enabled", isEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$reminderId", reminderId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteReminderAsync(int reminderId, CancellationToken cancellationToken = default)
    {
        var user = RequirePremium();
        await ExecuteUserCommandAsync(
            "DELETE FROM Reminders             WHERE ReminderId = $reminderId AND UserId = $userId;",
            cancellationToken,
            ("$reminderId", reminderId),
            ("$userId", user.UserId)).ConfigureAwait(false);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            await Task.Run(static () => { }, cancellationToken).ConfigureAwait(false);
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = """
                PRAGMA foreign_keys = ON;
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                    PasswordSalt TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    MinistryRole TEXT NOT NULL,
                    Phone TEXT NOT NULL DEFAULT '',
                    IsPremiumPreview INTEGER NOT NULL DEFAULT 0,
                    HasSeenWelcome INTEGER NOT NULL DEFAULT 0,
                    Theme TEXT NOT NULL DEFAULT 'System',
                    Language TEXT NOT NULL DEFAULT 'pt-BR',
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS Reminders (
                    ReminderId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
                    Title TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    Hour INTEGER NOT NULL CHECK (Hour BETWEEN 0 AND 23),
                    Minute INTEGER NOT NULL CHECK (Minute BETWEEN 0 AND 59),
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    IsDefault INTEGER NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Plans (
                    PlanId INTEGER NOT NULL PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    TotalDays INTEGER NOT NULL
                );
                CREATE TABLE IF NOT EXISTS UserPlans (
                    UserId INTEGER NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
                    PlanId INTEGER NOT NULL REFERENCES Plans(PlanId),
                    IsActive INTEGER NOT NULL DEFAULT 0,
                    StartedAt TEXT NOT NULL,
                    PRIMARY KEY (UserId, PlanId)
                );
                CREATE TABLE IF NOT EXISTS PlanDays (
                    UserId INTEGER NOT NULL,
                    PlanId INTEGER NOT NULL,
                    DayNumber INTEGER NOT NULL,
                    BookId INTEGER NOT NULL,
                    Chapter INTEGER NOT NULL,
                    PRIMARY KEY (UserId, PlanId, DayNumber, BookId, Chapter),
                    FOREIGN KEY (UserId, PlanId) REFERENCES UserPlans(UserId, PlanId) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS PlanProgress (
                    UserId INTEGER NOT NULL,
                    PlanId INTEGER NOT NULL,
                    DayNumber INTEGER NOT NULL,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    CompletedAt TEXT,
                    PRIMARY KEY (UserId, PlanId, DayNumber),
                    FOREIGN KEY (UserId, PlanId) REFERENCES UserPlans(UserId, PlanId) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS Notes (
                    NoteId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
                    BookId INTEGER NOT NULL,
                    Chapter INTEGER NOT NULL,
                    Verse INTEGER NOT NULL,
                    Text TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ReadingEvents (
                    UserId INTEGER NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
                    ReadDate TEXT NOT NULL,
                    PRIMARY KEY (UserId, ReadDate)
                );
                INSERT OR IGNORE INTO Plans (PlanId, Title, Description, TotalDays) VALUES
                    (1, 'Uma semana nos Salmos', 'Sete salmos para encontrar inspiração no dia a dia.', 7),
                    (2, 'Evangelho de João', 'Uma jornada de 21 dias pelo evangelho de João.', 21),
                    (3, 'Plano canônico (sequencial)', 'Gênesis ao Apocalipse em ordem tradicional. Simples de acompanhar; a primeira parte do ano concentra-se no Antigo Testamento.', 365),
                    (4, 'Plano cronológico (aproximado)', 'Livros organizados por períodos históricos para conectar acontecimentos. A sequência é aproximada, não uma cronologia versículo a versículo.', 365),
                    (5, 'Plano misto (AT + NT)', 'Leituras do Antigo e do Novo Testamento todos os dias. Estilo equilibrado, recomendado para manter variedade; alguns trechos do NT se repetem.', 365),
                    (6, 'Plano M’Cheyne', 'Plano clássico: Antigo Testamento uma vez, Novo Testamento duas vezes e Salmos em uma segunda leitura. Cerca de quatro capítulos por dia.', 365),
                    (7, 'Bíblia + Salmos e Provérbios', 'Leia a Bíblia inteira uma vez e releia Salmos e Provérbios ao longo do ano para dedicar mais tempo à oração e sabedoria.', 365);
                UPDATE Plans
                SET Title = 'Plano canônico (sequencial)',
                    Description = 'Gênesis ao Apocalipse em ordem tradicional. Simples de acompanhar; a primeira parte do ano concentra-se no Antigo Testamento.',
                    TotalDays = 365
                WHERE PlanId = 3;
                """;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await EnsureWelcomeColumnAsync(connection, cancellationToken).ConfigureAwait(false);
            _initialized = true;
            logger.LogInformation("Local account database initialized.");
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    private async Task<UserProfile> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT UserId, FirstName, LastName, MinistryRole, Phone, Email, IsPremiumPreview, Theme, Language, HasSeenWelcome
            FROM Users
            WHERE UserId = $userId;
            """;
        command.Parameters.AddWithValue("$userId", userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidDataException(Localizer.Get("A conta local não foi encontrada."));
        }

        return new UserProfile(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetInt32(6) == 1,
            reader.GetString(7),
            reader.GetString(8),
            reader.GetInt32(9) == 1);
    }

    private static async Task EnsureWelcomeColumnAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var hasWelcomeColumn = false;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(Users);";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (string.Equals(reader.GetString(1), "HasSeenWelcome", StringComparison.OrdinalIgnoreCase))
                {
                    hasWelcomeColumn = true;
                    break;
                }
            }
        }

        if (hasWelcomeColumn)
        {
            return;
        }

        await using var migration = connection.CreateCommand();
        migration.CommandText =
            "ALTER TABLE Users ADD COLUMN HasSeenWelcome INTEGER NOT NULL DEFAULT 1;";
        await migration.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        var connection = CreateConnection();
        await Task.Run(connection.Open, cancellationToken).ConfigureAwait(false);
        await using var foreignKeys = connection.CreateCommand();
        foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
        await foreignKeys.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static SqliteConnection CreateConnection() =>
        new(new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString());

    private async Task ExecuteUserCommandAsync(
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] parameters)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedDefaultRemindersAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int userId,
        CancellationToken cancellationToken)
    {
        foreach (var reminder in GetDefaultReminders())
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO Reminders (UserId, Title, Message, Hour, Minute, IsEnabled, IsDefault)
                VALUES ($userId, $title, $message, $hour, $minute, 1, 1);
                """;
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$title", reminder.Title);
            command.Parameters.AddWithValue("$message", reminder.Message);
            command.Parameters.AddWithValue("$hour", reminder.Hour);
            command.Parameters.AddWithValue("$minute", reminder.Minute);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static (string Title, string Message, int Hour, int Minute)[] GetDefaultReminders() =>
        Localizer.IsPortuguese ? PortugueseDefaultReminders : EnglishDefaultReminders;

    private static PrayerReminder LocalizeDefaultReminder(PrayerReminder reminder)
    {
        if (!reminder.IsDefault)
        {
            return reminder;
        }

        var currentDefaults = GetDefaultReminders();
        for (var index = 0; index < currentDefaults.Length; index++)
        {
            var portuguese = PortugueseDefaultReminders[index];
            var english = EnglishDefaultReminders[index];
            var previouslyLocalizedEnglish =
                (Title: Localizer.Get(portuguese.Title), Message: Localizer.Get(portuguese.Message));
            if ((reminder.Title == portuguese.Title && reminder.Message == portuguese.Message) ||
                (reminder.Title == english.Title && reminder.Message == english.Message) ||
                (reminder.Title == previouslyLocalizedEnglish.Title &&
                 reminder.Message == previouslyLocalizedEnglish.Message))
            {
                return reminder with
                {
                    Title = currentDefaults[index].Title,
                    Message = currentDefaults[index].Message
                };
            }
        }

        return reminder;
    }

    private static List<(int Day, int BookId, int Chapter)> BuildPlanEntries(
        int planId,
        IReadOnlyList<BookInfo> books)
    {
        if (planId == 1)
        {
            var psalms = books.Single(book => book.Code == "PSA");
            var psalmChapters = new[] { 1, 23, 27, 46, 91, 121, 139 };
            return psalmChapters
                .Select((chapter, index) => (index + 1, psalms.BookId, chapter))
                .ToList();
        }

        if (planId == 2)
        {
            var john = books.Single(book => book.Code == "JOH");
            return DistributeChapters(
                Enumerable.Range(1, john.ChapterCount).Select(chapter => (john.BookId, chapter)),
                21);
        }

        var oldTestamentBooks = books.TakeWhile(book => book.Code != "MAT").ToArray();
        var newTestamentBooks = books.SkipWhile(book => book.Code != "MAT").ToArray();
        var oldTestament = FlattenChapters(oldTestamentBooks);
        var newTestament = FlattenChapters(newTestamentBooks);
        var allChapters = oldTestament.Concat(newTestament).ToArray();

        return planId switch
        {
            3 => DistributeChapters(allChapters, 365),
            4 => DistributeChapters(FlattenChapters(GetChronologicalBookOrder(books)), 365),
            5 => BuildMixedPlan(oldTestament, newTestament),
            6 => BuildMcCheynePlan(oldTestament, newTestament, books),
            7 => BuildPsalmsAndProverbsPlan(allChapters, books),
            _ => []
        };
    }

    private static (int BookId, int Chapter)[] FlattenChapters(IEnumerable<BookInfo> books) =>
        books.SelectMany(book =>
                Enumerable.Range(1, book.ChapterCount).Select(chapter => (book.BookId, Chapter: chapter)))
            .ToArray();

    private static List<(int Day, int BookId, int Chapter)> DistributeChapters(
        IEnumerable<(int BookId, int Chapter)> chapters,
        int totalDays,
        int dayOffset = 0)
    {
        var passages = chapters.ToArray();
        return passages
            .Select((passage, index) =>
            {
                var day = (int)((long)index * totalDays / passages.Length);
                day = (day + dayOffset) % totalDays + 1;
                return (day, passage.BookId, passage.Chapter);
            })
            .ToList();
    }

    private static List<(int Day, int BookId, int Chapter)> BuildMixedPlan(
        (int BookId, int Chapter)[] oldTestament,
        (int BookId, int Chapter)[] newTestament)
    {
        var entries = new List<(int Day, int BookId, int Chapter)>();
        const int totalDays = 365;

        for (var dayIndex = 0; dayIndex < totalDays; dayIndex++)
        {
            var start = (int)((long)dayIndex * oldTestament.Length / totalDays);
            var end = (int)((long)(dayIndex + 1) * oldTestament.Length / totalDays);
            for (var index = start; index < end; index++)
            {
                entries.Add((dayIndex + 1, oldTestament[index].BookId, oldTestament[index].Chapter));
            }

            var newTestamentPassage = newTestament[dayIndex % newTestament.Length];
            entries.Add((dayIndex + 1, newTestamentPassage.BookId, newTestamentPassage.Chapter));
        }

        return entries;
    }

    private static List<(int Day, int BookId, int Chapter)> BuildMcCheynePlan(
        (int BookId, int Chapter)[] oldTestament,
        (int BookId, int Chapter)[] newTestament,
        IReadOnlyList<BookInfo> books)
    {
        var psalms = FlattenChapters([books.Single(book => book.Code == "PSA")]);
        var entries = DistributeChapters(oldTestament, 365);
        entries.AddRange(DistributeChapters(newTestament, 365, 90));
        entries.AddRange(DistributeChapters(newTestament, 365, 210));
        entries.AddRange(DistributeChapters(psalms, 365, 150));
        return entries;
    }

    private static List<(int Day, int BookId, int Chapter)> BuildPsalmsAndProverbsPlan(
        (int BookId, int Chapter)[] allChapters,
        IReadOnlyList<BookInfo> books)
    {
        var psalms = FlattenChapters([books.Single(book => book.Code == "PSA")]);
        var proverbs = FlattenChapters([books.Single(book => book.Code == "PRO")]);
        var entries = DistributeChapters(allChapters, 365);
        entries.AddRange(DistributeChapters(psalms, 365, 120));
        entries.AddRange(DistributeChapters(proverbs, 365, 240));
        return entries;
    }

    private static IReadOnlyList<BookInfo> GetChronologicalBookOrder(IReadOnlyList<BookInfo> books)
    {
        string[] approximateOrder =
        [
            "GEN", "JOB", "EXO", "LEV", "NUM", "DEU", "JOS", "JDG", "RUT",
            "1SA", "2SA", "1CH", "PSA", "1KI", "2KI", "2CH", "PRO", "ECC", "SNG",
            "JOL", "JON", "AMO", "HOS", "ISA", "MIC", "NAH", "ZEP", "HAB", "OBA",
            "JER", "LAM", "DAN", "EZK", "EZR", "HAG", "ZEC", "EST", "NEH", "MAL",
            "MAT", "MRK", "LUK", "JHN", "ACT", "JAS", "GAL", "1TH", "2TH", "1CO",
            "2CO", "ROM", "COL", "EPH", "PHM", "PHP", "1TI", "TIT", "2TI", "1PE",
            "2PE", "HEB", "JUD", "1JN", "2JN", "3JN", "REV"
        ];
        var ranks = approximateOrder
            .Select((code, index) => (code, index))
            .ToDictionary(item => item.code, item => item.index, StringComparer.OrdinalIgnoreCase);
        return books.OrderBy(book => ranks.TryGetValue(book.Code, out var rank) ? rank : int.MaxValue)
            .ThenBy(book => book.BookId)
            .ToArray();
    }

    private static void AddReminderParameters(
        SqliteCommand command,
        int userId,
        string title,
        string message,
        TimeSpan time)
    {
        command.Parameters.AddWithValue("$userId", userId);
        command.Parameters.AddWithValue("$title", title.Trim());
        command.Parameters.AddWithValue("$message", message.Trim());
        command.Parameters.AddWithValue("$hour", time.Hours);
        command.Parameters.AddWithValue("$minute", time.Minutes);
    }

    private UserProfile RequireAccount() =>
        CurrentUser ?? throw new UnauthorizedAccessException(Localizer.Get("Entre ou crie uma conta para acessar este recurso."));

    private UserProfile RequirePremium()
    {
        var user = RequireAccount();
        return HasPremiumAccess
            ? user
            : throw new UnauthorizedAccessException(Localizer.Get("Este recurso está disponível no Premium."));
    }

    private static void ValidateRegistration(AccountRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (string.IsNullOrWhiteSpace(registration.FirstName) ||
            string.IsNullOrWhiteSpace(registration.LastName) ||
            string.IsNullOrWhiteSpace(registration.MinistryRole))
        {
            throw new ArgumentException(Localizer.Get("Preencha nome, sobrenome e função no corpo de Cristo."));
        }

        if (registration.FirstName.Trim().Length > MaximumNameLength ||
            registration.LastName.Trim().Length > MaximumNameLength ||
            registration.MinistryRole.Trim().Length > MaximumMinistryRoleLength ||
            (registration.Phone?.Trim().Length ?? 0) > MaximumPhoneLength)
        {
            throw new ArgumentException(Localizer.Get("Confira o tamanho dos dados de perfil."));
        }

        if (string.IsNullOrWhiteSpace(registration.Email) ||
            registration.Email.Trim().Length > MaximumEmailLength ||
            !System.Net.Mail.MailAddress.TryCreate(registration.Email, out var address) ||
            !string.Equals(address.Address, registration.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Localizer.Get("Digite um e-mail válido."));
        }

        if (string.IsNullOrEmpty(registration.Password) ||
            registration.Password.Length < 8 ||
            registration.Password.Length > MaximumPasswordLength)
        {
            throw new ArgumentException(Localizer.Get("A senha deve ter entre 8 e 1024 caracteres."));
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static void ValidateNoteText(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > MaximumNoteLength)
        {
            throw new ArgumentException(Localizer.Get("A anotação deve ter entre 1 e 10000 caracteres."), nameof(text));
        }
    }

    private static void ValidateReminder(string title, string message, TimeSpan time)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(Localizer.Get("Informe um título e uma mensagem para o lembrete."));
        }

        if (title.Trim().Length > MaximumReminderTitleLength ||
            message.Trim().Length > MaximumReminderMessageLength)
        {
            throw new ArgumentException(Localizer.Get("O título deve ter até 100 caracteres e a mensagem até 1000."));
        }

        ValidateTime(time);
    }

    private static void ValidateProfile(string firstName, string lastName, string ministryRole, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(ministryRole))
        {
            throw new ArgumentException(Localizer.Get("Nome, sobrenome e função ministerial são obrigatórios."));
        }

        if (firstName.Trim().Length > MaximumNameLength ||
            lastName.Trim().Length > MaximumNameLength ||
            ministryRole.Trim().Length > MaximumMinistryRoleLength ||
            (phone?.Trim().Length ?? 0) > MaximumPhoneLength)
        {
            throw new ArgumentException(Localizer.Get("Confira o tamanho dos dados de perfil."));
        }
    }

    private static void ValidateTime(TimeSpan time)
    {
        if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(time));
        }
    }
}
