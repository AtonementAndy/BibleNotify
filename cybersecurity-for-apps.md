# Skill: Cybersecurity & Data Privacy Guardrails for .NET MAUI

**Context:** .NET MAUI (Android, iOS, Windows, MacCatalyst) with local SQLite (Microsoft.Data.Sqlite), Shell navigation, and MVVM.
**Goal:** Instruct the AI agent to never introduce common security vulnerabilities. Every code generation must respect these rules. Users must only access their own data. No hardcoded secrets. No exposed endpoints.

---

## 1. Hard Stops (Never Violate)

1. **NEVER** concatenate user input into SQL strings. Always use parameterized queries.
2. **NEVER** hardcode passwords, API keys, tokens, or connection strings in code. Use .NET Secret Manager (dev) or SecureStorage (runtime) or Azure Key Vault (backend only).
3. **NEVER** trust the client for authorization decisions. The backend (or local data layer) must enforce that a user only reads/writes their own rows.
4. **NEVER** expose local database files in plaintext. Encrypt at rest when sensitive (e.g., SQLCipher).
5. **NEVER** log credentials, tokens, or PII. Sanitize logs.
6. **NEVER** allow Guest or Non-Premium users to bypass tier checks via UI manipulation or direct service calls.

---

## 2. SQL Injection (OWASP A05:2025 – Injection)

**Rule:** Use SqliteCommand with Parameters.AddWithValue exclusively. No string interpolation is allowed.

**Correct Pattern:**

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM Notes WHERE UserId = @userId AND NoteId = @noteId";
    cmd.Parameters.AddWithValue("@userId", currentUserId);
    cmd.Parameters.AddWithValue("@noteId", noteId);

**Forbidden Pattern:**

    // NEVER DO THIS
    var sql = $"SELECT * FROM Notes WHERE UserId = {currentUserId}";

**Checklist:**
- [ ] Every SQL statement uses parameters for all variable inputs.
- [ ] No string.Format, $"" or + concatenation inside CommandText.
- [ ] LIKE queries use @param (e.g., WHERE Text LIKE '%' || @term || '%').

---

## 3. Broken Access Control (OWASP A01:2025 – #1 Risk)

**Rule:** Every data access operation must filter by the authenticated user’s ID. No exceptions.

**Correct Pattern (Service Layer):**

    public async Task<List<Note>> GetNotesAsync(int userId)
    {
        // userId comes from the authenticated session, NEVER from a client parameter
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Notes WHERE UserId = @userId";
        cmd.Parameters.AddWithValue("@userId", userId);
        // ...
    }

**Forbidden Patterns:**
- Passing userId from the UI as a parameter to a data method.
- Using WHERE 1=1 or omitting the UserId filter.
- Trusting IsPremiumPreview flag from the client without re-validating server-side (or local secure storage).

**Checklist:**
- [ ] Every SELECT, UPDATE, DELETE includes WHERE UserId = @currentUserId.
- [ ] ViewModels never expose a UserId property that the View can bind to and modify.
- [ ] Tier checks (Guest / Non-Premium / Premium) are enforced in the Service layer, not only by hiding UI elements.

---

## 4. Secrets Management (OWASP M1 – Improper Credential Usage)

**Rule:** Never hardcode secrets. Use the correct storage per environment.

| Environment | Correct Storage |
|-------------|-----------------|
| Development (local) | dotnet user-secrets (Secret Manager) |
| Production runtime (MAUI) | SecureStorage (native keychain/keystore) |
| Backend / API keys | Never in MAUI app. Backend only (Azure Key Vault, environment variables). |

**Correct Pattern (Runtime Secret):**

    // Store
    await SecureStorage.SetAsync("api_token", token);

    // Retrieve
    var token = await SecureStorage.GetAsync("api_token");

**Forbidden Patterns:**
- const string ApiKey = "sk-..."; in any .cs file.
- appsettings.json with secrets committed to Git.
- Using Azure Key Vault directly from MAUI (keys would be exposed in the binary).

**Checklist:**
- [ ] No secret strings in source code.
- [ ] SecureStorage used for runtime tokens.
- [ ] .gitignore includes secrets.json, appsettings.*.json (if used), .env.

---

## 5. Data at Rest (OWASP M9 – Insecure Data Storage)

**Rule:** The local SQLite database (bible.db and users.db) may contain user notes, reminders, and profile data. If the threat model includes device theft, encrypt the database.

**Recommended Approach:**
- Use SQLCipher (SQLitePCLRaw.bundle_e_sqlcipher) with a key derived from user credentials or stored in SecureStorage.
- For the offline Bible (bible.db), encryption is optional (public domain text).
- For user-generated content (Notes, Reminders, Users), encryption is mandatory for production.

**Checklist:**
- [ ] User database (users.db) is either encrypted or justified as non-sensitive.
- [ ] No plaintext sensitive data in AppData accessible via file manager.
- [ ] Backups exclude the database (Android: android:allowBackup="false" or backup rules).

---

## 6. Input Validation (OWASP M4 – Insufficient Input/Output Validation)

**Rule:** Validate all external inputs (user text, API responses, deep link parameters).

**Checklist:**
- [ ] Email format validated before registration.
- [ ] Password length >= 8, with complexity rules.
- [ ] Notes text length <= 5000 chars (prevent abuse).
- [ ] Deep link / route parameters validated (e.g., BookId is a valid book code).
- [ ] No raw HTML rendering from untrusted sources.

---

## 7. Communication Security (OWASP M5 – Insecure Communication)

**Rule:** All network calls (English Bible API, future payment API) must use HTTPS with certificate validation.

**Checklist:**
- [ ] HttpClient uses default TLS validation (never disable ServerCertificateCustomValidationCallback).
- [ ] No http:// endpoints.
- [ ] API rate limits respected (English Bible: bible-api.com).

---

## 8. Authentication & Session Security (OWASP M3 – Insecure Authentication)

**Rule:** Passwords are hashed with PBKDF2 + salt. Sessions are not persisted in plaintext.

**Correct Pattern (Already in project):**
- PBKDF2 with SHA-256, minimum 100,000 iterations.
- Salt: 16 random bytes per user.
- Hash: stored as salt:hash or separate columns.

**Checklist:**
- [ ] No plaintext passwords in memory longer than necessary.
- [ ] SecureStorage used for session tokens (if any).
- [ ] Logout clears all in-memory user data.

---

## 9. Pre-Flight Checklist for the Agent

Before emitting any code block, confirm:

- [ ] No SQL string concatenation. All queries use @parameters.
- [ ] Every data method filters by UserId from the authenticated session.
- [ ] No hardcoded secrets in code or config files.
- [ ] SecureStorage used for tokens/keys at runtime.
- [ ] Tier enforcement (Guest / Non-Premium / Premium) is in the Service layer.
- [ ] Input validation present for user-supplied strings.
- [ ] HttpClient uses HTTPS and default certificate validation.
- [ ] Logs do not contain PII, passwords, or tokens.

---

## 10. One-Line Summary for the Agent

**Never generate code that concatenates SQL, omits UserId filters, hardcodes secrets, or trusts client-side authorization. Every data access must be parameterized, scoped to the authenticated user, and validated.**