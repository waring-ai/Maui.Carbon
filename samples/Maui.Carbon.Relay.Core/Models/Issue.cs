namespace Maui.Carbon.Relay.Models;

/// <summary>
/// One unit of tracked work. Mutable: services change it in place and raise <c>IIssueService.Changed</c>, so every
/// screen that shows it refreshes.
/// </summary>
public sealed class Issue
{
    public required int Id { get; init; }

    /// <summary>The human key, project key + number ("ATL-142").</summary>
    public required string Key { get; init; }

    public required string ProjectId { get; set; }

    public required string Title { get; set; }

    public string Description { get; set; } = "";

    public IssueType Type { get; set; }

    public IssueStatus Status { get; set; }

    public IssuePriority Priority { get; set; }

    public IssueResolution Resolution { get; set; }

    public string? AssigneeId { get; set; }

    public required string ReporterId { get; init; }

    public string? Component { get; set; }

    public List<string> Labels { get; init; } = [];

    public int? StoryPoints { get; set; }

    public string? SprintId { get; set; }

    /// <summary>The key of the epic (or parent story) this issue belongs to.</summary>
    public string? ParentKey { get; set; }

    public DateTime Created { get; init; }

    public DateTime Updated { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>A reminder time of day for the due date, if any.</summary>
    public TimeSpan? ReminderTime { get; set; }

    public HashSet<string> WatcherIds { get; init; } = [];

    public List<string> ReproSteps { get; init; } = [];

    /// <summary>A captured stack trace (bugs only).</summary>
    public string? StackTrace { get; set; }

    /// <summary>The git branch the work happens on.</summary>
    public string? Branch { get; set; }

    public List<string> LinkedKeys { get; init; } = [];

    public List<Comment> Comments { get; init; } = [];

    public List<Attachment> Attachments { get; init; } = [];

    public List<ActivityEvent> Activity { get; init; } = [];

    public bool IsOpen => Status is not (IssueStatus.Done or IssueStatus.Cancelled);

    /// <summary>Open and past its due date at <paramref name="now"/>.</summary>
    public bool IsOverdue(DateTime now) => IsOpen && DueDate is { } due && due.Date < now.Date;

    public override string ToString() => $"{Key} {Title}";
}

/// <summary>A comment on an issue.</summary>
public sealed record Comment(string Id, string AuthorId, string Body, DateTime Created);

/// <summary>A file attached to an issue.</summary>
public sealed record Attachment(string Name, long SizeBytes, string UploaderId, DateTime Uploaded)
{
    /// <summary>The size for display ("1.2 MB").</summary>
    public string SizeText => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} B",
        < 1024 * 1024 => $"{SizeBytes / 1024.0:0.#} KB",
        _ => $"{SizeBytes / (1024.0 * 1024):0.#} MB",
    };
}

/// <summary>One entry in an issue's history (and the dashboard's activity stream).</summary>
public sealed record ActivityEvent(string Id, string IssueKey, string ActorId, ActivityKind Kind, string Text, DateTime When);

/// <summary>An item in the notification panel.</summary>
public sealed class AppNotification
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string Body { get; init; } = "";

    public FeedbackKind Kind { get; init; }

    public DateTime When { get; init; }

    public string? IssueKey { get; init; }

    public bool IsRead { get; set; }
}
