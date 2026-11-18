using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Services;

/// <summary>Which issues changed, so screens can refresh only when it matters to them.</summary>
public sealed class IssuesChangedEventArgs(IReadOnlyList<string> keys, string reason) : EventArgs
{
    public IReadOnlyList<string> Keys { get; } = keys;

    public string Reason { get; } = reason;
}

/// <summary>Issues: read, create, change and delete. Changes happen in place and raise <see cref="Changed"/>.</summary>
public interface IIssueService
{
    event EventHandler<IssuesChangedEventArgs>? Changed;

    /// <summary>A snapshot of every issue (no latency; for derived numbers).</summary>
    IReadOnlyList<Issue> All { get; }

    /// <summary>Every label in use, alphabetical.</summary>
    IReadOnlyList<string> Labels { get; }

    Issue? Find(string key);

    /// <summary>Loads every issue (with simulated latency, so lists show their loading state).</summary>
    Task<IReadOnlyList<Issue>> ListAsync(CancellationToken ct = default);

    Task<Issue?> GetAsync(string key, CancellationToken ct = default);

    Task<Issue> CreateAsync(IssueDraft draft, CancellationToken ct = default);

    Task SetStatusAsync(IEnumerable<Issue> issues, IssueStatus status, CancellationToken ct = default);

    Task AssignAsync(IEnumerable<Issue> issues, string? personId, CancellationToken ct = default);

    Task ResolveAsync(Issue issue, IssueResolution resolution, CancellationToken ct = default);

    /// <summary>Applies <paramref name="edit"/> and records <paramref name="summary"/> ("changed the due date") in the activity.</summary>
    Task UpdateAsync(Issue issue, Action<Issue> edit, string summary, CancellationToken ct = default);

    Task<Comment> AddCommentAsync(Issue issue, string body, CancellationToken ct = default);

    Task AddAttachmentAsync(Issue issue, string fileName, long sizeBytes, CancellationToken ct = default);

    Task RemoveAttachmentAsync(Issue issue, string fileName, CancellationToken ct = default);

    Task ToggleWatchAsync(Issue issue, CancellationToken ct = default);

    Task DeleteAsync(IEnumerable<Issue> issues, CancellationToken ct = default);

    /// <summary>Puts deleted issues back (the undo of <see cref="DeleteAsync"/>).</summary>
    Task RestoreAsync(IEnumerable<Issue> issues, CancellationToken ct = default);

    /// <summary>Renders <paramref name="issues"/> as CSV or JSON text.</summary>
    Task<string> ExportAsync(IEnumerable<Issue> issues, string format, CancellationToken ct = default);
}

/// <summary>Projects and the starred/archived state.</summary>
public interface IProjectService
{
    event EventHandler? Changed;

    IReadOnlyList<Project> All { get; }

    Project? Find(string? id);

    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct = default);

    Task<Project> CreateAsync(Project project, CancellationToken ct = default);

    void ToggleStar(Project project);

    /// <summary>True when <paramref name="key"/> is free and 2–5 capital letters.</summary>
    bool IsKeyAvailable(string key);
}

/// <summary>Workspace members and the signed-in user.</summary>
public interface IPeopleService
{
    IReadOnlyList<Person> All { get; }

    Person Me { get; }

    Person? Find(string? id);

    /// <summary>The display name, or "Unassigned" for null.</summary>
    string NameOf(string? id);
}

/// <summary>Sprints per project.</summary>
public interface ISprintService
{
    event EventHandler? Changed;

    IReadOnlyList<Sprint> ForProject(string projectId);

    Sprint? Active(string projectId);

    Sprint? Find(string? id);

    Task StartAsync(Sprint sprint, CancellationToken ct = default);

    Task CompleteAsync(Sprint sprint, CancellationToken ct = default);
}

/// <summary>The header's notification panel feed.</summary>
public interface INotificationFeed
{
    event EventHandler? Changed;

    IReadOnlyList<AppNotification> Items { get; }

    int UnreadCount { get; }

    void MarkRead(AppNotification notification);

    void MarkAllRead();

    void Clear();

    void Push(AppNotification notification);

    /// <summary>Adds a plausible new notification (the app calls this on a timer so the feed feels live).</summary>
    AppNotification SimulateIncoming();
}

/// <summary>The project the shell is scoped to (board, backlog, dashboard). Changed from the header switcher.</summary>
public interface IWorkspace
{
    event EventHandler? CurrentProjectChanged;

    Project CurrentProject { get; }

    void SelectProject(string projectId);
}

/// <summary>The outcome of a sign-in attempt.</summary>
public sealed record SignInResult(bool Succeeded, string? Field, string? Message);

/// <summary>Mock authentication: any workspace email with a password of 6+ characters.</summary>
public interface IAuthService
{
    bool IsSignedIn { get; }

    /// <summary>Step one: does an account exist for this email?</summary>
    Task<SignInResult> CheckEmailAsync(string email, CancellationToken ct = default);

    Task<SignInResult> SignInAsync(string email, string password, bool remember, CancellationToken ct = default);

    void SignOut();
}

/// <summary>An AI-generated summary with the points it drew on.</summary>
public sealed record AssistantSummary(string Text, IReadOnlyList<string> Highlights, IReadOnlyList<string> IssueKeys);

/// <summary>An AI suggestion of who should take an issue, with the reason shown in the AI label.</summary>
public sealed record AssigneeSuggestion(string PersonId, string Reason, double Confidence);

/// <summary>A pair of issues the assistant thinks describe the same problem.</summary>
public sealed record DuplicateCandidate(Issue Issue, Issue Duplicate, double Similarity);

/// <summary>The mock AI assistant. Results are computed from the data, not from a model.</summary>
public interface IAssistantService
{
    Task<AssistantSummary> SummarizeSprintAsync(string projectId, CancellationToken ct = default);

    Task<AssistantSummary> SummarizeWeekAsync(string personId, CancellationToken ct = default);

    Task<AssigneeSuggestion?> SuggestAssigneeAsync(Issue issue, CancellationToken ct = default);

    Task<IReadOnlyList<DuplicateCandidate>> FindDuplicatesAsync(CancellationToken ct = default);
}

/// <summary>User preferences shown on the settings screen.</summary>
public sealed class AppSettings
{
    public string DisplayName { get; set; } = "Maya Chen";

    public string Email { get; set; } = "maya.chen@relay.dev";

    public string TimeZone { get; set; } = "America/Toronto";

    public string DateFormat { get; set; } = "MMM d, yyyy";

    public string DefaultProjectId { get; set; } = "atl";

    public ThemeChoice Theme { get; set; } = ThemeChoice.White;

    public bool ReduceMotion { get; set; }

    public bool CompactTables { get; set; }

    public bool EmailOnAssign { get; set; } = true;

    public bool EmailOnMention { get; set; } = true;

    public bool EmailOnStatus { get; set; }

    public bool DailyDigest { get; set; } = true;

    public int DigestHour { get; set; } = 8;

    public bool SlackEnabled { get; set; } = true;

    public string SlackChannel { get; set; } = "#atlas-dev";

    public bool GitHubEnabled { get; set; } = true;

    public string ApiToken { get; set; } = "rly_live_9f2c41d07be84a6c93e1";

    public string WebhookUrl { get; set; } = "https://hooks.relay.dev/t/atlas";
}

/// <summary>Loads and saves <see cref="AppSettings"/>.</summary>
public interface ISettingsService
{
    AppSettings Current { get; }

    /// <summary>Raised after <see cref="SaveAsync"/> replaced <see cref="Current"/>.</summary>
    event EventHandler? Changed;

    Task SaveAsync(AppSettings settings, CancellationToken ct = default);

    /// <summary>Issues a new API token and returns it.</summary>
    Task<string> RegenerateTokenAsync(CancellationToken ct = default);
}
